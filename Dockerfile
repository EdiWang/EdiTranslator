FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
LABEL maintainer="edi.wang@outlook.com"
LABEL repo="https://github.com/EdiWang/EdiTranslator"

USER app
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build-with-spa
RUN curl -fsSL https://deb.nodesource.com/setup_22.x | bash - && \
    apt-get install -y nodejs
FROM build-with-spa AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

COPY ["Edi.Translator.csproj", "."]
RUN dotnet restore "./Edi.Translator.csproj"
COPY . .
WORKDIR "/src/."
RUN dotnet build "./Edi.Translator.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./Edi.Translator.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Edi.Translator.dll"]
