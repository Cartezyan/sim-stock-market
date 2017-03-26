import { BrowserModule } from '@angular/platform-browser';
import { NgModule } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { HttpModule } from '@angular/http';
import { NgbModule } from '@ng-bootstrap/ng-bootstrap';
import { StockQuoteDataModule } from '../stock-quote-data';
import { AppComponent } from './app.component';
import { environment } from '../../environments/environment';

@NgModule({
  declarations: [
    AppComponent
  ],
  imports: [
    BrowserModule,
    FormsModule,
    HttpModule,
    NgbModule.forRoot(),
    StockQuoteDataModule.create(environment.stubMode)
  ],
  providers: [],
  bootstrap: [AppComponent]
})
export class AppModule { }
