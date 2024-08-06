FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 5000
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["thetaedgecloud-the-ai-factor.csproj", "api/"]
RUN dotnet restore "api/thetaedgecloud-the-ai-factor.csproj"

WORKDIR "/src/api"
COPY . .

RUN dotnet build "thetaedgecloud-the-ai-factor.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "thetaedgecloud-the-ai-factor.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

#WORKDIR "/src/api"
#ADD onnx_models /app/publish/onnx_models

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "thetaedgecloud-the-ai-factor.dll"]
