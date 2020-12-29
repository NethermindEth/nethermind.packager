using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Nethermind.Packager.Core.Services.Options;
using Nethermind.Packager.Core.Services.Storage;
using Nethermind.Packager.Core.Services.Validators;

namespace Nethermind.Packager.Core.Services
{
    public class PackageUploader : IPackageUploader
    {
        private readonly IStorageUploader _storageUploader;
        private readonly IPackageValidator _packageValidator;
        private readonly IHashGenerator _hashGenerator;
        private readonly IOptions<AccessOptions> _accessOptions;
        private readonly IOptions<PackageOptions> _packageOptions;

        public PackageUploader(IStorageUploader storageUploader,
            IPackageValidator packageValidator,
            IHashGenerator hashGenerator,
            IOptions<AccessOptions> accessOptions,
            IOptions<PackageOptions> packageOptions)
        {
            _storageUploader = storageUploader;
            _packageValidator = packageValidator;
            _hashGenerator = hashGenerator;
            _accessOptions = accessOptions;
            _packageOptions = packageOptions;
        }

        public async Task UploadAsync(IReadOnlyCollection<IFormFile> files, string apiKey)
        {
            ValidateApiKeyOrFail(apiKey);
            ValidateFilesOrFail(files);
            var filesPackage = await GetFilesPackageAsync(files, _packageOptions.Value.SignatureExtension);
            ValidateSignatureOrFail(filesPackage, apiKey);
            
            var filesToUpload = new List<File> {filesPackage.Package};
            if (_packageOptions.Value.ValidateSignature)
            {
                filesToUpload.Add(filesPackage.Signature);
            }

            await UploadAsync(filesToUpload);
        }

        private void ValidateApiKeyOrFail(string apiKey)
        {
            if (string.IsNullOrWhiteSpace(apiKey))
            {
                throw new ArgumentException("Empty API key.", nameof(apiKey));
            }

            var hash = _hashGenerator.Hash(apiKey);
            if (_accessOptions.Value.ApiKeysHashes.All(h => h != hash))
            {
                throw new InvalidOperationException("Invalid API key.");
            }
        }

        private void ValidateFilesOrFail(IReadOnlyCollection<IFormFile> files)
        {
            if (files == null || !files.Any())
            {
                throw new ArgumentException("Missing files.", nameof(files));
            }

            if (files.Count > 2)
            {
                throw new ArgumentException($"Too many files ({files.Count}).", nameof(files));
            }

            var availableExtensions = _packageOptions.Value.Extensions.Select(e => e.Key.ToLowerInvariant()).ToList();
            var filesExtensions = files.Select(f => GetExtension(f.FileName).ToLowerInvariant()).ToList();

            if (!availableExtensions.Intersect(filesExtensions).Any())
            {
                throw new ArgumentException("Provided package has invalid extension.", nameof(files));
            }

            if (!_packageOptions.Value.ValidateSignature)
            {
                return;
            }

            var signatureExtension = _packageOptions.Value.SignatureExtension;
            if (files.Count == 1 || !filesExtensions.Contains(signatureExtension))
            {
                throw new ArgumentException("Missing file containing signature " +
                                            $"'(.{_packageOptions.Value.SignatureExtension})' to validate a package.",
                    nameof(files));
            }
        }
        
        private void ValidateSignatureOrFail(FilesPackage filesPackage, string apiKey)
        {
            if (!_packageOptions.Value.ValidateSignature)
            {
                return;
            }

            if (_packageValidator.IsValid(filesPackage.Package.Bytes, filesPackage.Signature.Bytes, apiKey))
            {
                return;
            }

            throw new InvalidOperationException("Invalid package signature.");
        }

        private async Task UploadAsync(IEnumerable<File> files)
            => await Task.WhenAll(files.Select(f => _storageUploader.UploadAsync(f.Bytes, f.Name)));

        private static async Task<FilesPackage> GetFilesPackageAsync(IReadOnlyCollection<IFormFile> files,
            string signatureExtension)
        {
            var filesPackage = new FilesPackage();

            foreach (var file in files)
            {
                var bytes = await GetBytesAsync(file);
                if (IsSignature(file.FileName, signatureExtension))
                {
                    filesPackage.Signature = new File(file.FileName, bytes);
                }
                else
                {
                    filesPackage.Package = new File(file.FileName, bytes);
                }
            }

            return filesPackage;
        }

        private static async Task<byte[]> GetBytesAsync(IFormFile file)
        {
            using (var memoryStream = new MemoryStream())
            {
                await file.CopyToAsync(memoryStream);

                return memoryStream.ToArray();
            }
        }

        private static bool IsSignature(string name, string signatureExtension)
            => GetExtension(name) == signatureExtension;

        private static string GetExtension(string name)
        {
            if(name.EndsWith("gz"))
            {
                return "tar.gz";
            } 
            else
            {
                return name.Split('.')?.LastOrDefault();
            }
        }

        private class FilesPackage
        {
            public File Package { get; set; }
            public File Signature { get; set; }
        }

        private class File
        {
            public string Name { get; }
            public byte[] Bytes { get; }

            public File(string name, byte[] bytes)
            {
                Name = name;
                Bytes = bytes;
            }
        }
    }
}