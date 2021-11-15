#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:6.0-alpine3.14-amd64 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:6.0-alpine3.14-amd64 AS build
WORKDIR /src
COPY ["Payment.Historical.Server/Payment.Historical.Server.csproj", "Payment.Historical.Server/"]
RUN dotnet restore "Payment.Historical.Server/Payment.Historical.Server.csproj"
COPY . .
WORKDIR "/src/Payment.Historical.Server"
RUN dotnet build "Payment.Historical.Server.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Payment.Historical.Server.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Payment.Historical.Server.dll"]