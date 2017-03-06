FROM microsoft/dotnet:1.0-runtime
ARG source
WORKDIR /app
COPY ${source:-obj/Docker/publish} .
ENTRYPOINT ["dotnet", "SimStockMarket.TraderBot.dll"]
