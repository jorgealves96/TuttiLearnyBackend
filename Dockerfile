# Stage 1: Build the application
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY . .
RUN dotnet publish "LearningAppNetCoreApi.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Stage 2: Create the final image
FROM mcr.microsoft.com/dotnet/aspnet:9.0
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "LearningAppNetCoreApi.dll"]