# https://hub.docker.com/_/microsoft-dotnet
FROM mcr.microsoft.com/dotnet/sdk:3.1 AS build
WORKDIR /source

EXPOSE 80

# copy csproj and restore as distinct layers
COPY Matbox.sln .
COPY . ./Matbox/
RUN dotnet restore ./Matbox/Matbox.WEB/Matbox.WEB.csproj
RUN dotnet restore ./Matbox/Matbox.BLL/Matbox.BLL.csproj 
RUN dotnet restore ./Matbox/Matbox.DAL/Matbox.DAL.csproj 
RUN dotnet restore ./Matbox/Matbox.Tests/Matbox.Tests.csproj

# copy everything else and build app
WORKDIR /source/Matbox
RUN dotnet publish -c release -o /app --no-restore

# final stage/image
FROM mcr.microsoft.com/dotnet/aspnet:3.1
WORKDIR /app
COPY --from=build /app ./
ENTRYPOINT ["dotnet", "Matbox.WEB.dll", "Matbox.BLL.dll", "Matbox.DAL.dll"]