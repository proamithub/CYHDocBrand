# ==============================================================================
# CYH DoctorHub — Production Multi-Stage Dockerfile (.NET 10.0 LTS)
# ==============================================================================

# Stage 1: Runtime Base Image (.NET 10 ASP.NET Core)
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production

# Stage 2: SDK Image for Build & Publish
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy csproj files for optimal layer caching
COPY ["CYHDocBrandModel/CYHDocBrandModel.csproj", "CYHDocBrandModel/"]
COPY ["CYHDocBrandData/CYHDocBrandData.csproj", "CYHDocBrandData/"]
COPY ["CYHDocBrandRepo/CYHDocBrandRepo.csproj", "CYHDocBrandRepo/"]
COPY ["CYHDocBrandSMS/CYHDocBrandSMS.csproj", "CYHDocBrandSMS/"]
COPY ["CYHDocBrandUI/CYHDocBrandUI.csproj", "CYHDocBrandUI/"]

# Restore packages
RUN dotnet restore "CYHDocBrandUI/CYHDocBrandUI.csproj"

# Copy full source tree
COPY . .

# Build and Publish Release package
WORKDIR "/src/CYHDocBrandUI"
RUN dotnet publish "CYHDocBrandUI.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Stage 3: Production Lightweight Container
FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "CYHDocBrandUI.dll"]
