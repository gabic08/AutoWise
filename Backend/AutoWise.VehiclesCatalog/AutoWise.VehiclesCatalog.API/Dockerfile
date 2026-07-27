FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY . .
RUN dotnet publish "AutoWise.VehiclesCatalog/AutoWise.VehiclesCatalog.API/AutoWise.VehiclesCatalog.API.csproj" -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "AutoWise.VehiclesCatalog.API.dll"]