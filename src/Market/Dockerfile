FROM microsoft/dotnet:1.0-runtime
ARG source
WORKDIR /app
COPY out/ .
ENTRYPOINT ["dotnet", "SimStockMarket.Market.dll"]
