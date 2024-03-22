FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 443
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["API/API.csproj", "API/"]
COPY ["DataTrainer/DataTrainer.csproj", "DataTrainer/"]

RUN dotnet restore "API/API.csproj"
RUN dotnet restore "DataTrainer/DataTrainer.csproj"
COPY . .

# Install a stable version of Entity Framework Core tools
RUN dotnet tool install --global dotnet-ef --version 7.0.14

# Add dotnet tools directory to PATH
ENV PATH="$PATH:/root/.dotnet/tools"

WORKDIR "/src"

RUN dotnet ef migrations add InitialCreate -s API -p Persistence

FROM build AS publish
WORKDIR "/src/API"
RUN dotnet publish "API.csproj" -c Release -o /app/publish/API /p:UseAppHost=false

WORKDIR "/src/DataTrainer"
RUN dotnet publish "DataTrainer.csproj" -c Release -o /app/publish/DataTrainer

# TODO: Remove this. It's helpful for debugging

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish/API .

ENTRYPOINT ["dotnet", "API.dll"]