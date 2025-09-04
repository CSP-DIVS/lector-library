# build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ./backend/Csp.Api ./Csp.Api
RUN dotnet restore ./Csp.Api && dotnet publish ./Csp.Api -c Release -o /app

# run
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app .
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080
ENTRYPOINT ["dotnet","Csp.Api.dll"]
