# https://hub.docker.com/_/microsoft-dotnet
FROM mcr.microsoft.com/dotnet/sdk:3.1 AS build
WORKDIR /source

EXPOSE 80

# copy csproj and restore as distinct layers
COPY MatBoxApi.sln .
COPY MatBoxApi/MatBoxApi.csproj ./MatBoxApi/
RUN dotnet restore MatBoxApi/MatBoxApi.csproj 

# copy everything else and build app
COPY MatBoxApi/. ./MatBoxApi/
WORKDIR /source/MatBoxApi
RUN dotnet publish -c release -o /app --no-restore

# final stage/image
FROM mcr.microsoft.com/dotnet/aspnet:3.1
WORKDIR /app
COPY --from=build /app ./
ENTRYPOINT ["dotnet", "MatBoxApi.dll"]