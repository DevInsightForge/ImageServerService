FROM mcr.microsoft.com/dotnet/nightly/sdk:8.0-alpine-aot AS build
WORKDIR /src

COPY ["ImageServer/ImageServer.csproj", "ImageServer/"]
ARG TARGETARCH
RUN dotnet restore "./ImageServer/ImageServer.csproj" -r linux-musl-"$TARGETARCH"
COPY . .
RUN dotnet publish --no-restore -o /app/publish "./ImageServer/ImageServer.csproj"

FROM mcr.microsoft.com/dotnet/nightly/runtime-deps:8.0-alpine-aot AS final
WORKDIR /app
EXPOSE 8080
COPY --from=build /app/publish .
ENTRYPOINT ["./ImageServer"]
