# Stage 1: Build the application
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# --- Install EF Core tools in the build stage ---
# This ensures dotnet-ef is available for migrations
RUN dotnet tool install --global dotnet-ef
ENV PATH="$PATH:/root/.dotnet/tools"
# ---------------------------------------------

# Copy the project file first to leverage Docker layer caching
COPY ["LearningAppNetCoreApi.csproj", "."]
# Restore dependencies (FIXED TYPO HERE)
RUN dotnet restore "./LearningAppNetCoreApi.csproj"

# Copy the rest of the application code
COPY . .
WORKDIR "/src/."
# Build the application
RUN dotnet build "LearningAppNetCoreApi.csproj" -c Release -o /app/build

# Stage 2: Publish the application
FROM build AS publish
# Publish the application, ensuring it's self-contained and ready to run
RUN dotnet publish "LearningAppNetCoreApi.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Stage 3: Create the final image
FROM mcr.microsoft.com/dotnet/aspnet:9.0
WORKDIR /app
# Copy the published application from the publish stage
COPY --from=publish /app/publish .

# Copy the startup script and make it executable
COPY startup.sh .
RUN chmod +x startup.sh

# Set the entrypoint to your startup script
CMD ["./startup.sh"]
