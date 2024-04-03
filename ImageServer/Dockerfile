FROM mcr.microsoft.com/dotnet/sdk:8.0-alpine AS build
RUN apk update && \
	apk add --no-cache clang zlib-dev

ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["ImageServer/ImageServer.csproj", "ImageServer/"]
RUN dotnet restore "./ImageServer/ImageServer.csproj"
COPY . .
WORKDIR "/src/ImageServer"
RUN dotnet build "./ImageServer.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./ImageServer.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=true

FROM mcr.microsoft.com/dotnet/runtime-deps:8.0-alpine AS final
WORKDIR /app
EXPOSE 8080
COPY --from=publish /app/publish .
ENTRYPOINT ["./ImageServer"]