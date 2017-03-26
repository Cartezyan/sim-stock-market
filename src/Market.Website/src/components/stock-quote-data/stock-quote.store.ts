import { environment } from '../../environments/environment';
import { Injectable } from '@angular/core';
import { Subject } from 'rxjs/Subject';
import { Observable } from 'rxjs/Observable';
import { Subscriber } from 'rxjs/Subscriber';
import { StockQuote } from './model';
import * as io from 'socket.io-client';

@Injectable()
export class StockQuoteDataStore {

    private _bus: SocketIOClient.Socket;
    private _quotes: StockQuote[] = [];
    private _quotesStream: Observable<StockQuote[]>;

    get quotes(): Observable<StockQuote[]> {

        this._bus = io.connect(environment.realtimeStockQuotesUrl);

        return new Observable<StockQuote[]>(observer => {

            this._bus.on('quote', data => {
                const quote = JSON.parse(data);

                const wasChanged = this.updateQuote(quote);

                if (wasChanged) {
                    observer.next(this._quotes);
                }
            });

            return () => {
                this._bus.disconnect();
            };
        });
    }

    private updateQuote(quote: StockQuote): boolean {
        let existing = this._quotes.find(x => x.Symbol === quote.Symbol);

        if (!existing) {
            this._quotes.push(quote);
            existing = quote;
        }

        // Ignore stale updates
        if (existing.AsOf > quote.AsOf) {
            return false;
        }

        existing.AsOf = quote.AsOf;
        existing.AsOfDate = (quote.AsOf) ? new Date(quote.AsOf) : null;
        existing.Ask = quote.Ask;
        existing.Bid = quote.Bid;
        existing.Price = quote.Price;

        return true;
    }
}