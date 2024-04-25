# Get base SDK image from Microsoft
FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build-env

WORKDIR /app
# Copy the solution file and restore any dependencies (via NuGet)
COPY *.sln ./
COPY APIs/*.csproj ./APIs/
COPY BusinessObjects/*.csproj ./BusinessObjects/
COPY DataAccess/*.csproj ./DataAccess/
RUN dotnet restore

# Copy the project files and build our release
COPY . ./
RUN dotnet publish APIs/APIs.csproj -c Release -o out

# Generate runtime image
FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS runtime

# Set the working directory
WORKDIR /app

# Copy the build artifacts from the previous stage
COPY --from=build-env /app/out ./

# Set the entry point to run the API project
ENTRYPOINT ["dotnet", "APIs.dll"]