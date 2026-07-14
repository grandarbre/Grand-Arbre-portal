FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy csproj and restore dependencies
COPY ["Grand Arbre portal.csproj", "."]
RUN dotnet restore "Grand Arbre portal.csproj"

# Copy everything else and build
COPY . .
RUN dotnet publish "Grand Arbre portal.csproj" -c Release -o /app/publish

# Runtime image
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

# CREATE DATA DIRECTORY WITH PROPER PERMISSIONS - THIS IS THE IMPORTANT PART!
RUN mkdir -p /app/data && chmod -R 777 /app/data

# Copy published files
COPY --from=build /app/publish .

# Set environment variables
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production

# Expose port
EXPOSE 8080

# Start the app
ENTRYPOINT ["dotnet", "Grand Arbre portal.dll"]