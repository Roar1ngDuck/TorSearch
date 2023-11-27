# Use the Microsoft SDK image to build the project
FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /app

# Copy csproj and restore any dependencies (via NuGet)
COPY ["TorSearch.Scraper/TorSearch.Scraper.csproj", "./"]
RUN dotnet restore "TorSearch.Scraper.csproj"

# Copy the project files and build the release
COPY . .
RUN dotnet publish "TorSearch.Scraper/TorSearch.Scraper.csproj" -c Release -o /app/publish

# Generate the runtime image
FROM mcr.microsoft.com/dotnet/runtime:7.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "TorSearch.Scraper.dll"]
