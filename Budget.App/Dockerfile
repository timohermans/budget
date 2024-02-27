FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["Budget.App/Budget.App.csproj", "Budget.App/"]
COPY ["Budget.Core/Budget.Core.csproj", "Budget.Core/"]
RUN dotnet restore "Budget.App/Budget.App.csproj"
COPY . .
WORKDIR "/src/Budget.App"
RUN dotnet build "Budget.App.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "Budget.App.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Budget.App.dll"]
