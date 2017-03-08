namespace SimStockMarket.Market.Handlers
{
    interface IHandler
    {
        void Handle(object message);
    }

    public abstract class Handler<TMessage> : IHandler
    {
        void IHandler.Handle(object message)
        {
            Handle((TMessage)message);
        }

        public abstract void Handle(TMessage message);
    }
}
