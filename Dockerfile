FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build-env
WORKDIR /App

# Copy everything
COPY . ./
# Restore as distinct layers
RUN dotnet restore
# Build and publish a release
RUN dotnet publish -c Release -o out

# Build runtime image
FROM mcr.microsoft.com/dotnet/runtime:6.0
MAINTAINER Rickard Stolt <rickard@stolt.one>
WORKDIR /App
COPY --from=build-env /App/out .
ENTRYPOINT ["dotnet", "tibber-influxdb.dll"]