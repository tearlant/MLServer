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

RUN dotnet ef migrations add InitialCreate -s API -p Persistence --verbose

FROM build AS publish
WORKDIR "/src/API"
RUN dotnet publish "API.csproj" -c Release -o /app/publish/API /p:UseAppHost=false

WORKDIR "/src/DataTrainer"
RUN dotnet publish "DataTrainer.csproj" -c Release -o /app/publish/DataTrainer

# TODO: Remove this. It's helpful for debugging

#RUN pwd
#RUN ls -la /app
#RUN ls -la /app/publish
#RUN ls -la /app/publish/API
#RUN ls -la /app/publish/DataTrainer

#FROM base AS final
#WORKDIR /app
#COPY --from=publish /app/publish/API .

#ENTRYPOINT ["dotnet", "API.dll"]

# Generate the Dockerfile for serving the app
RUN echo "FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base" > /app/publish/API/Dockerfile
RUN echo "WORKDIR /app" >> /app/publish/API/Dockerfile
RUN echo "ARG CERT_PATH" >> /app/publish/API/Dockerfile
RUN echo "ARG KEY_PATH" >> /app/publish/API/Dockerfile
RUN echo "COPY $CERT_PATH /app/cert.pem" >> /app/publish/API/Dockerfile
RUN echo "COPY $KEY_PATH /app/key.pem" >> /app/publish/API/Dockerfile

RUN echo "COPY . ./" >> /app/publish/API/Dockerfile
RUN echo "EXPOSE 443" >> /app/publish/API/Dockerfile
RUN echo "EXPOSE 80" >> /app/publish/API/Dockerfile
RUN echo "ENTRYPOINT [\"dotnet\", \"API.dll\"]" >> /app/publish/API/Dockerfile

# In theory, the documentation I've read says I can extract the files from the images, but I can't make that work,
# so the workaround is to have the container running indefinitely, and having the build server killing the container
# after the artifacts have been extracted.
CMD ["sleep", "infinity"]