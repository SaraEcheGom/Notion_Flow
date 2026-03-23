FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build

WORKDIR /src
COPY ["NotionFlow.Api/NotionFlow.Api.csproj", "NotionFlow.Api/"]
RUN dotnet restore "NotionFlow.Api/NotionFlow.Api.csproj"

COPY . .
WORKDIR "/src/NotionFlow.Api"
RUN dotnet build "NotionFlow.Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "NotionFlow.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM runtime AS final
WORKDIR /app
COPY --from=publish /app/publish .
EXPOSE 5000
ENTRYPOINT ["dotnet", "NotionFlow.Api.dll"]