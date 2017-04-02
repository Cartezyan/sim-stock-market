using System;
using System.Collections.Generic;
using System.Linq;
using Serilog;
using MongoDB.Driver;
using MongoDB.Bson;

namespace SimStockMarket.Market.Handlers
{
    public abstract class TradeOfferHandler<T> : Handler<T>
    {
        private static ILogger Log = Serilog.Log.ForContext<TradeOfferHandler<T>>();

        private readonly IMessageBus _bus;
        private readonly IMongoCollection<TradeOffer> _offers;
        private readonly TradeRequestHandler _tradeHandler;

        public TradeOfferHandler(
                IMessageBus bus,
                IMongoCollection<TradeOffer> offers,
                TradeRequestHandler tradeHandler
            )
        {
            _bus = bus;
            _offers = offers;
            _tradeHandler = tradeHandler;
        }

        protected IEnumerable<TradeOffer> GetOffersBySymbol(string symbol)
        {
            return _offers.AsQueryable().Where(x => x.Symbol == symbol);
        }

        protected void SubmitOffer(TradeOffer offer)
        {
            if (offer == null)
                throw new ArgumentNullException(nameof(offer));

            if (string.IsNullOrWhiteSpace(offer.Symbol))
                throw new ArgumentException("Invalid symbol");

            if (offer.Price <= 0)
                throw new ArgumentException("Offer price must be positive");

            offer.Timestamp = DateTime.UtcNow;

            // If I knew how to actually use MongoDB, I could get rid of this conditional
            if (offer._id == ObjectId.Empty)
            {
                _offers.InsertOne(offer);
            }
            else
            {
                _offers.ReplaceOne(
                    x => x.Symbol == offer.Symbol && x.TraderId == offer.TraderId,
                    offer
                );
            }

            _bus.Publish("offer", offer);

            _tradeHandler.ReevaluateQuote(offer.Symbol);

            Log.Information("{@offer}", offer);
        }

    }
}
