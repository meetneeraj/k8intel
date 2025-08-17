# === STAGE 1: Build Environment ===
# Use the official .NET SDK image which contains all build tools.
# Using specific versions like 8.0 is better for reproducibility than 'latest'.
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build-env
WORKDIR /app

# Copy the project files and restore dependencies first. This is a Docker caching optimization.
# It means Docker won't re-download all NuGet packages every time you change a .cs file.
COPY *.csproj ./
RUN dotnet restore

# Copy the rest of the source code
COPY . ./

# Publish the application, creating a Release build optimized for running.
RUN dotnet publish -c Release -o /app/publish


# === STAGE 2: Final Runtime Image ===
# Use the minimal and hardened ASP.NET runtime image. It doesn't contain the SDK,
# making it much smaller and more secure.
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app

# Copy the published output from the build-env stage
COPY --from=build-env /app/publish .

# Expose the port the application will listen on inside the container.
# It's a best practice to run as a non-privileged user and on a non-standard port.
# We will tell our app to listen on port 8080.
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080

# The command to run when the container starts.
ENTRYPOINT ["dotnet", "K8intel.dll"]