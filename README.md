# The Simulated Stock Market

The Simulated Stock Market application is a highly-distributed, loosely-coupled, cloud-ready, and whatever other buzzword-of-the-day application created as a sandbox for exercising new technologies, techniques, patterns, practices, or tools.  It's meant to be complex enough to support more advanced development techniques, yet simple enough to easily understand and modify wherever is needed.

Nothing is actually built yet.  In fact, I've only just started thinking about this for the past few weeks.  But, the project will evolve in the open right here.

## The Domain

After lots of consideration, the simplest and most complex domain I could think of is a stock market.

**It's simple** because it's driven by two events:  buying and selling stocks.

**It's complex** because there are _sooooooo_ many variables that can affect the _decision_ to buy or sell any given stock at a particular price.  Not to mention, there are so many derivatives: indexes, options, mutual funds, ETFs... and plenty of others that I have never heard of.

Before continuing, it's probably worth pointing out (if you haven't realized already) that I really have no expertise in this area.  It'd be nice to make the domain and activities of the system as realistic as possible, but ultimately it just needs to be simple enough to understand quickly and complex enough to leverage advanced development techniques... it doesn't have to actually represent reality.


### High-Level Concepts

To start, here are the high-level concepts that will be represented in the system:

* **The Market (aka the Exchange)**:  this is where trades occur; in other words, where stocks are bought and sold.
  * **The Ledger**: the history of all trades that have occurred, how many shares, what cost, and who was involved
* **The Stocks**: these are the commodities being traded
* **The Trader**: this is the actor placing the trades  _(can be either automated/AI or an actual person)_


## Design Overview

Technically speaking, here are the parts that are going to compose the trading system:

* **The Market**: a long-running "service" that keeps track of the current price and quantity of all stocks.
* **Trade Bots**: automated bots that act as *Traders* in the system, with varying levels of sophistication.
* **Trading Clients**: various UIs (web, mobile, desktop) that expose manual trading functionality.
* **Market Administration**: a back-office system to enable management (adding and removing) of stocks as well as the manipulation of their attributes (prices, evaluations, etc.)
  * **API**: The automatable, non-UI API that exposes all back-office functionality
  * **UI**: the UI that consumes the API so that people can interact with it
* **Market History/Monitoring/Analysis**: real-time (read-only) monitoring and analysis of all trading activity.  Will probably (eventually) have a couple of parts:
  * **Data Lake & Big Data processing**: Oh yeah.
  * **API**: an API that exposes ability to query the data lake
  * **UI**: awesome dashboard with all sorts of impressive animated charts and gauges and other real-timey things


Note that since this example is intended to evolve and become more complex over time, this list is bound to change.
