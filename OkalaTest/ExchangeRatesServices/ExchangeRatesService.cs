using System.Text.Json;
using Core.ExchangeServices;

namespace OkalaTest.ExchangeRatesServices;

public class ExchangeRatesService : IExchangeRatesService
{
    private readonly string _apiKey;
    private readonly IHttpClientFactory _clientFactory;

    public ExchangeRatesService(IConfiguration configuration, IHttpClientFactory clientFactory)
    {
        _clientFactory = clientFactory;
        var rawApiKey = configuration["ExchangeRate:ApiKey"];
        _apiKey = rawApiKey?.Trim() ?? throw new ArgumentNullException("API key not configured.");
    }

    public async Task<ExchangeRatesResponse> GetHistoricalRatesAsync(string date, string baseCurrency,
        IEnumerable<string> symbols,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var client = _clientFactory.CreateClient("ExchangeRate");
            var symbolsParam = string.Join(",", symbols);
            var requestUri = $"{date}?access_key={_apiKey}&base=EUR&symbols={symbolsParam}";
            var response =
                await client.GetFromJsonAsync<ExchangeRatesResponse>(requestUri, cancellationToken);

            if (response is { Success: false })
                throw new HttpRequestException(
                    "ExchangeRates API returned error"
                );

            if (response != null) return response;
        }
        catch (OperationCanceledException)
        {
            throw new TimeoutException("The request to ExchangeRates API was canceled (likely due to timeout).");
        }
        catch (HttpRequestException ex)
        {
            throw new ApplicationException("Failed to call ExchangeRates API.", ex);
        }
        catch (NotSupportedException ex)
        {
            throw new ApplicationException("The content type is not supported for deserialization.", ex);
        }
        catch (JsonException ex)
        {
            throw new ApplicationException("Failed to parse the response from ExchangeRates API.", ex);
        }
        catch (Exception ex)
        {
            throw new ApplicationException("Unexpected error while calling ExchangeRates API.", ex);
        }

        throw new InvalidOperationException("ExchangeRates API returned empty or malformed response.");
    }
}