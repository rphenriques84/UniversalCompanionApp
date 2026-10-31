FROM mcr.microsoft.com/dotnet/sdk:10.0.403-alpine3.24 AS build
WORKDIR /repo
COPY . .
RUN dotnet publish src/Companion.Frontend/Companion.Frontend.csproj -c Release -o /app

FROM mcr.microsoft.com/dotnet/aspnet:10.0.14-alpine3.24 AS runtime
WORKDIR /app
COPY --from=build /app .
USER $APP_UID
EXPOSE 8080
ENTRYPOINT ["dotnet", "Companion.Frontend.dll"]
