import { Component } from '@angular/core';
import { Observable } from 'rxjs/Observable';
import { StockQuote, StockQuoteDataStore } from '../stock-quote-data';

@Component({
  selector: 'app-root',
  templateUrl: './app.component.html'
})
export class AppComponent {
  title = 'Stock Market Simulator';

  quotes: StockQuote[] = [];

  constructor(private quotesStore: StockQuoteDataStore) {
    quotesStore.quotes.subscribe((quotes) => this.quotes = quotes);
  }

}
