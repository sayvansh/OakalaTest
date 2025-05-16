using Core.CryptoServices;
using Core.ExchangeServices;

namespace OkalaTest.Crypto;

public class CryptoQuoteService(ICryptoPriceService cryptoPriceService, IExchangeRatesService exchangeRatesService)
    : ICryptoQuoteService
{
    private static readonly string[] TargetCurrencies = ["EUR", "BRL", "GBP", "AUD"];


    public async Task<CryptoQuoteResult> GetCryptoPricesAsync(string cryptoSymbol,
        CancellationToken cancellationToken = default)
    {
        var usdResponse = await cryptoPriceService.GetCryptoPriceInUsdAsync(cryptoSymbol, cancellationToken);
        var usdPrice = usdResponse.PriceInUsd;

        var exchangeRates = await exchangeRatesService.GetHistoricalRatesAsync(
            "latest", 
            "EUR",
            TargetCurrencies.Append("USD"),
            cancellationToken
        );

        if (!exchangeRates.Rates.TryGetValue("USD", out var eurToUsdRate))
            throw new InvalidOperationException("ExchangeRates response missing USD rate.");

        var usdToOthers = TargetCurrencies.ToDictionary(
            currency => currency,
            currency =>
            {
                if (!exchangeRates.Rates.TryGetValue(currency, out var eurToCurrency))
                    throw new InvalidOperationException($"ExchangeRates response missing {currency} rate.");

                var usdToCurrency = eurToCurrency / eurToUsdRate;
                var cryptoValueInCurrency = usdPrice * usdToCurrency;
                return Math.Round(cryptoValueInCurrency, 4);
            }
        );

        usdToOthers["USD"] = Math.Round(usdPrice, 4);

        return new CryptoQuoteResult
        {
            CryptoCode = cryptoSymbol.ToUpperInvariant(),
            Prices = usdToOthers
        };
    }
}