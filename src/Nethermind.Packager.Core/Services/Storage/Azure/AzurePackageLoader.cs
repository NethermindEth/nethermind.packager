using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Serialization;
using Microsoft.Extensions.Options;
using Nethermind.Packager.Core.Services.Dto;
using Nethermind.Packager.Core.Services.Options;

namespace Nethermind.Packager.Core.Services.Storage.Azure
{
    public class AzurePackageLoader : IPackageLoader
    {
        private readonly IStorageClient _storageClient;
        private readonly IOptions<PackageOptions> _packageOptions;

        public AzurePackageLoader(IStorageClient storageClient,
            IOptions<PackageOptions> packageOptions)
        {
            _storageClient = storageClient;
            _packageOptions = packageOptions;
        }

        public async Task<IEnumerable<PackageDto>> GetAllAsync()
        {
            var content = await _storageClient.GetContentAsync();

            return string.IsNullOrWhiteSpace(content) ? Enumerable.Empty<PackageDto>() : GetPackages(content);
        }

        private IEnumerable<PackageDto> GetPackages(string content)
        {
            var serializer = new XmlSerializer(typeof(EnumerationResults));
            using (var reader = new StringReader(content))
            {
                var result = (EnumerationResults) (serializer.Deserialize(reader));

                return result.Blobs.Blob.Select(Map).Where(p => !(p is null)).ToList();
            }
        }

        //nethermind-darwin-amd64-1.2.3-5ff1a481.tar.gz
        //nethermind-darwin-amd64-1.2.4-unstable-5ff1a481.tar.gz
        private PackageDto Map(Blob blob)
        {
            var extension = blob.Name.Split('.')?.LastOrDefault()?.ToLowerInvariant();
            if (!IsExtensionValid(extension))
            {
                return null;
            }

            var parts = blob.Name?.Split('-');
            if (!ArePartsValid(parts))
            {
                return null;
            }

            var partialPackage = CreatePartialPackage(parts);
            var releases = _packageOptions.Value.Releases.Select(r => r.ToLowerInvariant());

            return releases.Contains(partialPackage.Release.ToLowerInvariant())
                ? CreatePackage(partialPackage, extension, blob)
                : null;
        }

        private PackageDto CreatePartialPackage(string[] parts)
        {
            var packageName = parts[0];
            var platform = parts[1];
            var arch = parts[2];
            var version = parts[3];
            var commit = parts[4];
            var release = _packageOptions.Value.DefaultRelease;
            if (parts.Length == 6)
            {
                release = parts[4];
                commit = parts[5];
            }

            return new PackageDto
            {
                Name = $"{packageName} {version}",
                Platform = platform,
                Arch = arch,
                Version = version,
                Commit = commit?.Split('.')?.FirstOrDefault(),
                Release = release,
            };
        }

        private PackageDto CreatePackage(PackageDto partialPackage, string extension, Blob blob)
        {
            partialPackage.Kind = GetKind(extension);
            partialPackage.Size = blob.Properties.ContentLength;
            partialPackage.Url = blob.Url;
            partialPackage.PublishedAt = DateTime.Parse(blob.Properties.LastModified);
            partialPackage.Checksum = GetMd5Hex(blob);

            return partialPackage;
        }

        private bool IsExtensionValid(string extension)
        {
            if (extension == _packageOptions.Value.SignatureExtension.ToLowerInvariant())
            {
                return false;
            }

            var extensions = _packageOptions.Value.Extensions.Select(e => e.Key.ToLowerInvariant());

            return extensions.Contains(extension);
        }

        private bool ArePartsValid(string[] parts)
            => !(parts is null) && (parts.Length == 5 || parts.Length == 6);

        private static string GetMd5Hex(Blob blob)
            => BitConverter.ToString(Convert.FromBase64String(blob.Properties.ContentMD5))
                .Replace("-", string.Empty).ToLowerInvariant();

        private string GetKind(string extension)
            => _packageOptions.Value.Extensions.Single(e => e.Key.ToLowerInvariant() == extension).Value;

        #region XML

        [XmlRoot(ElementName = "Properties")]
        public class Properties
        {
            [XmlElement(ElementName = "Last-Modified")]
            public string LastModified { get; set; }

            [XmlElement(ElementName = "Etag")] public string Etag { get; set; }

            [XmlElement(ElementName = "Content-Length")]
            public long ContentLength { get; set; }

            [XmlElement(ElementName = "Content-Type")]
            public string ContentType { get; set; }

            [XmlElement(ElementName = "Content-Encoding")]
            public string ContentEncoding { get; set; }

            [XmlElement(ElementName = "Content-Language")]
            public string ContentLanguage { get; set; }

            [XmlElement(ElementName = "Content-MD5")]
            public string ContentMD5 { get; set; }

            [XmlElement(ElementName = "Cache-Control")]
            public string CacheControl { get; set; }

            [XmlElement(ElementName = "BlobType")] public string BlobType { get; set; }

            [XmlElement(ElementName = "LeaseStatus")]
            public string LeaseStatus { get; set; }
        }

        [XmlRoot(ElementName = "Blob")]
        public class Blob
        {
            [XmlElement(ElementName = "Name")] public string Name { get; set; }
            [XmlElement(ElementName = "Url")] public string Url { get; set; }

            [XmlElement(ElementName = "Properties")]
            public Properties Properties { get; set; }

        }

        [XmlRoot(ElementName = "Blobs")]
        public class Blobs
        {
            [XmlElement(ElementName = "Blob")] public List<Blob> Blob { get; set; }
        }

        [XmlRoot(ElementName = "EnumerationResults")]
        public class EnumerationResults
        {
            [XmlElement(ElementName = "Blobs")] public Blobs Blobs { get; set; }

            [XmlElement(ElementName = "NextMarker")]
            public string NextMarker { get; set; }

            [XmlAttribute(AttributeName = "ContainerName")]
            public string ContainerName { get; set; }
        }

        #endregion models
    }
}