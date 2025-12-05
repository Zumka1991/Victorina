FROM mcr.microsoft.com/dotnet/aspnet:10.0-preview AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:10.0-preview AS build
WORKDIR /src
COPY ["src/Victorina.Bot/Victorina.Bot.csproj", "src/Victorina.Bot/"]
COPY ["src/Victorina.Application/Victorina.Application.csproj", "src/Victorina.Application/"]
COPY ["src/Victorina.Infrastructure/Victorina.Infrastructure.csproj", "src/Victorina.Infrastructure/"]
COPY ["src/Victorina.Domain/Victorina.Domain.csproj", "src/Victorina.Domain/"]
RUN dotnet restore "src/Victorina.Bot/Victorina.Bot.csproj"
COPY . .
WORKDIR "/src/src/Victorina.Bot"
RUN dotnet build "Victorina.Bot.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Victorina.Bot.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Victorina.Bot.dll"]
