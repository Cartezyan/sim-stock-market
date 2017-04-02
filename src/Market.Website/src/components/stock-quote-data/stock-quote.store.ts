import { environment } from '../../environments/environment';
import { Injectable } from '@angular/core';
import { Http } from '@angular/http';
import { Subject } from 'rxjs/Subject';
import { Observable } from 'rxjs/Observable';
import { Subscriber } from 'rxjs/Subscriber';
import 'rxjs/add/operator/toPromise';
import { StockQuote } from './model';
import * as io from 'socket.io-client';

@Injectable()
export class StockQuoteDataStore {

    private _bus: SocketIOClient.Socket;
    private _quotes: StockQuote[] = null;
    private _quotesStream: Observable<StockQuote[]>;

    get quotes(): Observable<StockQuote[]> {
        if (!this._quotesStream) {
            this._quotes = [];
            this.loadQuotes(environment.stockSymbolsApiUrl)
                .then(() => this.loadQuotes(environment.stockQuoteApiUrl));

            this._quotesStream = this.startRealTimeQuotes(environment.realtimeStockQuotesUrl);
        }

        return this._quotesStream;
    }

    constructor(private _http: Http) {
    }

    private loadQuotes(url: string): Promise<void> {
        return this.get<StockQuote[]>(url)
            .then(symbols => symbols.forEach(this.updateQuote.bind(this)));
    }

    private get<T>(url: string): Promise<T> {
        return this._http.get(url)
            .toPromise()
            .then(x => <T>x.json());
    }

    private startRealTimeQuotes(url: string): Observable<StockQuote[]> {
        this._bus = io.connect(url);

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
        let existing = this._quotes.find(x => x.symbol === quote.symbol);

        if (!existing) {
            this._quotes.push(quote);
            existing = quote;
        }

        // Ignore stale updates
        if (existing.asOf > quote.asOf) {
            return false;
        }

        existing.asOf = quote.asOf;
        existing.asOfDate = (quote.asOf) ? new Date(quote.asOf) : null;
        existing.ask = quote.ask;
        existing.bid = quote.bid;
        existing.price = quote.price;

        return true;
    }
}