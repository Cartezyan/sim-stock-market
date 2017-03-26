export interface StockQuote {
    Symbol: string;
    AsOf?: string;
    AsOfDate?: Date;
    Price?: number;
    Bid?: number;
    Ask?: number;
}