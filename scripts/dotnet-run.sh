#!/bin/bash
export ASPNETCORE_ENVIRONMENT=local
cd src/Nethermind.Packager.Web
dotnet run --no-restore