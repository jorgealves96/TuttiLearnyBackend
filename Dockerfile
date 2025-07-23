# Cache buster v1
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# --- Build Stage ---
# Copy ONLY the main API project file first
COPY LearningAppNetCoreApi.csproj .

# Restore dependencies for ONLY the main API project
RUN dotnet restore "LearningAppNetCoreApi.csproj"

# Copy the rest of the source code
COPY . .

# Publish the main API project
RUN dotnet publish "LearningAppNetCoreApi.csproj" -c Release -o /app/publish

# --- Final Stage ---
# Use the smaller ASP.NET runtime image
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app
COPY --from=build /app/publish .

# Expose port 8080. Cloud Run automatically sends requests to this port.
ENV PORT=8080
EXPOSE 8080

# The command to run the application
ENTRYPOINT ["dotnet", "LearningAppNetCoreApi.dll"]