# gmde-inverter-scraper
Scraping your own inverters data from the very poor GMDE portal website.

A working example on how to scrape structured data from the website, since there is no official API, and the export functionality is severely limited.

Yes, the site really runs on HTTP and uses username+password authentication.. If you own one of these inverters, you also know how incredibly unsafe the default username and password they give you is.
Let's just say I won't be buying another inverter from GMDE anytime soon :)

## The pull action
hardcoded to go back 1 month from today, and pull the most granular data there is.
The website provides the data as JSON, with datapoints each 5 minutes. All datapoints are always returned:
* PV Generation
* Load consumption
* Grid feed-in
* Grid supply
* Battery storage

## The read action
I've shown a simple example on how to parse and aggregate some data, but you could just use the raw data file as input to some other tool or script.
