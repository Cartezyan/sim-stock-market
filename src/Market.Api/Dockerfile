FROM microsoft/dotnet:1.0-runtime
ARG source
WORKDIR /app
COPY ${source:-obj/Docker/publish} .

EXPOSE 5000

ENTRYPOINT ["dotnet", "SimStockMarket.Market.Api.dll"]
