#################
## Build stage ##
#################
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build-stage

WORKDIR /app

COPY . .

RUN dotnet publish --self-contained false -p:PublishSingleFile=false

######################
## Production stage ##
######################
FROM mcr.microsoft.com/dotnet/aspnet:8.0

WORKDIR /app

COPY --from=build-stage /app/bin/Release/net8.0/linux-x64/publish/ /app

EXPOSE 8080

CMD ["./GroupOrder"]