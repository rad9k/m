# BASE
#
# This stage is used when running from VS in fast mode (Default for Debug configuration)

FROM mcr.microsoft.com/dotnet/aspnet:10.0-nanoserver-ltsc2022 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

# BUILD
#
# This stage is used to build the service project

FROM mcr.microsoft.com/dotnet/sdk:10.0-nanoserver-ltsc2022 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["m0_console/m0_console.csproj", "m0_console/"]
RUN dotnet restore "./m0_console/m0_console.csproj"
COPY . .
WORKDIR "/src/m0_console"
RUN dotnet build "./m0_console.csproj" -c $BUILD_CONFIGURATION -o /app/build

# PUBLISH
#
# This stage is used to publish the service project to be copied to the final stage

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./m0_console.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# FINAL
#
# This stage is used in production or when running from VS in regular mode

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "m0_console.dll"]