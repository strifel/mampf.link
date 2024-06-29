#################
## Build stage ##
#################
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build-stage

WORKDIR /app

RUN dotnet tool install --global dotnet-ef

RUN ln -s /root/.dotnet/tools/dotnet-ef /usr/local/bin

COPY . .

RUN dotnet publish --self-contained false -p:PublishSingleFile=false

RUN dotnet ef migrations bundle -o DBMigrations

######################
## Production stage ##
######################
FROM mcr.microsoft.com/dotnet/aspnet:8.0

WORKDIR /app

COPY --from=build-stage /app/bin/Release/net8.0/linux-x64/publish/ /app

COPY --from=build-stage /app/DBMigrations /app/DBMigrations

EXPOSE 8080

CMD ["./GroupOrder"]