export interface StockQuote {
    name?: string;
    symbol: string;
    asOf?: string;
    asOfDate?: Date;
    price?: number;
    bid?: number;
    ask?: number;
}