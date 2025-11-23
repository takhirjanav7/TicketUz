# -------------------- Build stage --------------------
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ["MovieSystem.Api.csproj", "./"]
RUN dotnet restore "MovieSystem.Api.csproj"

COPY . .
RUN dotnet publish "MovieSystem.Api.csproj" -c Release -o /app/publish

# -------------------- Runtime stage --------------------
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://+:80
EXPOSE 80

ENTRYPOINT ["dotnet", "MovieSystem.Api.dll"]
