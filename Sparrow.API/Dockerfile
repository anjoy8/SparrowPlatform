#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:5.0-buster-slim AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:5.0-buster-slim AS build 
WORKDIR /src
COPY ["SparrowPlatform.API/SparrowPlatform.API.csproj", "SparrowPlatform.API/"]
COPY ["SparrowPlatform.Application/SparrowPlatform.Application.csproj", "SparrowPlatform.Application/"]
COPY ["SparrowPlatform.Infrastruct/SparrowPlatform.Infrastruct.csproj", "SparrowPlatform.Infrastruct/"]
COPY ["SparrowPlatform.Domain/SparrowPlatform.Domain.csproj", "SparrowPlatform.Domain/"]
RUN dotnet restore "SparrowPlatform.API/SparrowPlatform.API.csproj"
COPY . .
WORKDIR "/src/SparrowPlatform.API"
RUN dotnet build "SparrowPlatform.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "SparrowPlatform.API.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "SparrowPlatform.API.dll"]