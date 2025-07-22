# Stage 1: Build the application
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY ["LearningAppNetCoreApi.csproj", "."]
RUN dotnet restore "./LearningAppNetCoreApi.csproj"
COPY . .
WORKDIR "/src/."
RUN dotnet build "LearningAppNetCoreApi.csproj" -c Release -o /app/build

# Stage 2: Publish the application
FROM build AS publish
RUN dotnet publish "LearningAppNetCoreApi.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Stage 3: Create the final image
FROM mcr.microsoft.com/dotnet/aspnet:9.0
WORKDIR /app
COPY --from=publish /app/publish .

# --- Add these lines to install EF Core tools and copy the script ---
RUN dotnet tool install --global dotnet-ef
ENV PATH="$PATH:/root/.dotnet/tools"
COPY startup.sh .
RUN chmod +x startup.sh
# --------------------------------------------------------------------

# Change ENTRYPOINT to use the new startup script
CMD ["./startup.sh"]