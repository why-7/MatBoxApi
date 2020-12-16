# https://hub.docker.com/_/microsoft-dotnet
FROM mcr.microsoft.com/dotnet/sdk:3.1 AS build
WORKDIR /source

EXPOSE 80

# copy csproj and restore as distinct layers
COPY Matbox.sln .
COPY Matbox/Matbox.csproj ./Matbox/
RUN dotnet restore Matbox/Matbox.csproj 

# copy everything else and build app
COPY Matbox/. ./Matbox/
WORKDIR /source/Matbox
RUN dotnet publish -c release -o /app --no-restore

# final stage/image
FROM mcr.microsoft.com/dotnet/aspnet:3.1
WORKDIR /app
COPY --from=build /app ./
ENTRYPOINT ["dotnet", "Matbox.dll"]