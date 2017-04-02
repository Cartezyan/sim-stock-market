import { Injectable } from '@angular/core';
import { Subject } from 'rxjs/Subject';
import { Observable } from 'rxjs/Observable';
import { Subscriber } from 'rxjs/Subscriber';
import { StockQuote } from './model';
import { StockQuoteDataStore } from './stock-quote.store';

const PriceBase = 200;
const UpdateChance = 0.25;
const UpdateFrequency = 250;

@Injectable()
export class StockQuoteDataStoreStub extends StockQuoteDataStore {

    get quotes(): Observable<StockQuote[]> {

        // tslint:disable-next-line:no-console
        console.info('Using stub stock quote data store');

        let quotes: Array<StockQuote> =
            [ 'GOOG', 'MSFT', 'INTC', 'IBM', 'APPL', 'ADBE', 'TSLA', 'AMZN', 'NFLX', 'DIS', 'GE' ]
                .sort()
                .map(this.generateQuote);

        return new Observable<StockQuote[]>((observer: Subscriber<StockQuote[]>) => {
            setInterval(() => {

                quotes = quotes.map(quote =>
                    // give a low chance of modifying anything
                    (Math.random() < UpdateChance) ? this.update(quote) : quote
                );

                observer.next(quotes);
            }, UpdateFrequency);
        });
    }

    private generateQuote(symbol: string): StockQuote {
        const price = PriceBase * Math.random(),
              asOf = new Date();

        return {
            symbol: symbol,
            price: price,
            asOf: asOf.toDateString(),
            asOfDate: asOf,
            ask: price + 0.01,
            bid: price - 0.01,
        };
    }

    private update({ symbol: symbol, bid: bid, ask: ask, price: price }: StockQuote): StockQuote {

        // lower the ask OR raise the bid
        if (Math.random() > .50) {
            ask -= Math.random() / 10;
        } else {
            bid += Math.random() / 10;
        }

        // do we have a deal?  then that's the new price!
        if (bid >= ask) {
            price = bid;
            ask = price + 0.01;
            bid = price - 0.01;
        }

        const asOf = new Date();

        return {
            symbol: symbol,
            asOf: asOf.toDateString(),
            asOfDate: asOf,
            price: price,
            bid: bid,
            ask: ask,
        };
    }

}