import { Injectable } from '@angular/core';
import { Subject } from 'rxjs/Subject';
import { Observable } from 'rxjs/Observable';
import { Subscriber } from 'rxjs/Subscriber';
import { StockQuote } from './model';
import { IStockQuoteDataStore } from './stock-quote.store';

@Injectable()
export class StockQuoteDataStoreStub implements IStockQuoteDataStore {

    get quotes(): Observable<StockQuote[]> {

        const quotes: Array<StockQuote> = [
            { Symbol: 'GOOG', Price: 200 * Math.random() },
            { Symbol: 'MSFT', Price: 200 * Math.random() },
            { Symbol: 'INTL', Price: 200 * Math.random() },
            { Symbol: 'APPL', Price: 200 * Math.random() },
        ];

        return new Observable<StockQuote[]>((observer: Subscriber<StockQuote[]>) => {
            setInterval(() => observer.next(this.randomize(quotes)), 500);
        });
    }

    randomize(quotes: StockQuote[]): StockQuote[] {
        quotes.forEach(x => {

            // give a low chance of modifying anything
            if (Math.random() > .25) {
                return;
            }

            x.AsOf = new Date().toDateString();

            if (!x.Ask) {
                x.Ask = x.Price + 0.01;
            }
            if (!x.Bid) {
                x.Bid = x.Price - 0.01;
            }

            // lower the ask OR raise the bid
            if (Math.random() > .50) {
                x.Ask -= Math.random() / 10;
            } else {
                x.Bid += Math.random() / 10;
            }

            // do we have a deal?  then that's the new price!
            if (x.Bid >= x.Ask) {
                x.Price = x.Bid;
                x.Ask = x.Price + 0.01;
                x.Bid = x.Price - 0.01;
            }
        });

        return JSON.parse(JSON.stringify(quotes));
    }

}