namespace SimStockMarket.Market.Handlers
{
    public abstract class Handler<TMessage>
    {
        public abstract void Handle(TMessage message);
    }
}
