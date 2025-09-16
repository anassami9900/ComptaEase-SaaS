# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy csproj and restore dependencies
COPY *.csproj ./
RUN dotnet restore

# Copy source code and build
COPY . .
RUN dotnet publish -c Release -o /app/publish

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

# Create wwwroot directory
RUN mkdir -p wwwroot/bulletins

# Copy published application
COPY --from=build /app/publish .

# Expose port
EXPOSE 80
EXPOSE 443

ENTRYPOINT ["dotnet", "ComptaEase.API.dll"]