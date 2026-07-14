FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build 
WORKDIR /src 
COPY ["Grand Arbre portal.csproj", "."] 
RUN dotnet restore "Grand Arbre portal.csproj" 
COPY . . 
RUN dotnet publish "Grand Arbre portal.csproj" -c Release -o /app/publish 
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime 
WORKDIR /app 
COPY --from=build /app/publish . 
ENV ASPNETCORE_URLS=http://+:8080 
ENV ASPNETCORE_ENVIRONMENT=Production 
ENV DataDirectory=/app/data 
EXPOSE 8080 
ENTRYPOINT ["dotnet", "Grand Arbre portal.dll"] 
