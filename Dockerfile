FROM mcr.microsoft.com/dotnet/sdk:2.1 AS build
WORKDIR /app
COPY . .
RUN dotnet build src/Nethermind.Packager.Web
RUN dotnet publish src/Nethermind.Packager.Web -c Release -o out --no-restore 

FROM mcr.microsoft.com/dotnet/aspnet:2.1
WORKDIR /app
COPY --from=build /app/src/Nethermind.Packager.Web/out .
ENV ASPNETCORE_URLS http://*:5000
ENV ASPNETCORE_ENVIRONMENT docker
EXPOSE 5000
ENTRYPOINT dotnet Nethermind.Packager.Web.dll