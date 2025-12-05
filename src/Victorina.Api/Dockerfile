FROM mcr.microsoft.com/dotnet/aspnet:10.0-preview AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:10.0-preview AS build
WORKDIR /src
COPY ["src/Victorina.Api/Victorina.Api.csproj", "src/Victorina.Api/"]
COPY ["src/Victorina.Application/Victorina.Application.csproj", "src/Victorina.Application/"]
COPY ["src/Victorina.Infrastructure/Victorina.Infrastructure.csproj", "src/Victorina.Infrastructure/"]
COPY ["src/Victorina.Domain/Victorina.Domain.csproj", "src/Victorina.Domain/"]
RUN dotnet restore "src/Victorina.Api/Victorina.Api.csproj"
COPY . .
WORKDIR "/src/src/Victorina.Api"
RUN dotnet build "Victorina.Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Victorina.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Victorina.Api.dll"]
