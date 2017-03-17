import { Simstockmarkter.Market.WebsitePage } from './app.po';

describe('simstockmarkter.market.website App', () => {
  let page: Simstockmarkter.Market.WebsitePage;

  beforeEach(() => {
    page = new Simstockmarkter.Market.WebsitePage();
  });

  it('should display message saying app works', () => {
    page.navigateTo();
    expect(page.getParagraphText()).toEqual('app works!');
  });
});
