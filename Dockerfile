# Build stage
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy csproj and restore dependencies
COPY ["AutoGestPro.csproj", "./"]
RUN dotnet restore

# Copy everything else and build
COPY . .
RUN dotnet publish -c Release -o /app/publish

# Runtime stage
FROM mcr.microsoft.com/dotnet/runtime:9.0
WORKDIR /app

# Install GTK dependencies and Graphviz
RUN apt-get update && apt-get install -y \
    libgtk-3-0 \
    libgtk-3-dev \
    graphviz \
    && rm -rf /var/lib/apt/lists/*

# Copy built app
COPY --from=build /app/publish .

# Set entrypoint
ENTRYPOINT ["dotnet", "AutoGestPro.dll"]