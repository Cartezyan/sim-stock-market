import { NgModule, ModuleWithProviders } from '@angular/core';
import { StockQuoteDataStore } from './stock-quote.store';
import { StockQuoteDataStoreStub } from './stock-quote.store.stub';

@NgModule({
  declarations: [ ],
  providers: [ StockQuoteDataStore ],
})
export class StockQuoteDataModule {

  static create(isStub: boolean): ModuleWithProviders {
    return { ngModule: isStub ? StockQuoteDataStubModule : StockQuoteDataModule };
  }

}

@NgModule({
  declarations: [  ],
  providers: [{ provide: StockQuoteDataStore, useClass: StockQuoteDataStoreStub }],
})
export class StockQuoteDataStubModule {
}