using Core.CryptoServices;

namespace OkalaTest.CoinMarketCapCryptoServices;

public class CoinMarketCapCryptoPriceService : ICryptoPriceService
{
    private readonly string _apiKey;
    private readonly IHttpClientFactory _clientFactory;


    public CoinMarketCapCryptoPriceService(IConfiguration configuration, IHttpClientFactory clientFactory)
    {
        _clientFactory = clientFactory;
        var rawApiKey = configuration["CoinMarketCap:ApiKey"];
        _apiKey = rawApiKey?.Trim() ?? throw new ArgumentNullException("API key not configured.");
    }

    public async Task<CryptoPriceResult> GetCryptoPriceInUsdAsync(string cryptoSymbol,
        CancellationToken cancellationToken = default)
    {
        var client = _clientFactory.CreateClient("CoinMarketCap");
        client.DefaultRequestHeaders.Add("X-CMC_PRO_API_KEY", _apiKey);
        var requestUri = $"cryptocurrency/quotes/latest?symbol={cryptoSymbol.ToUpper()}&convert=USD";

        HttpResponseMessage response;
        try
        {
            response = await client.GetAsync(requestUri, cancellationToken);
        }
        catch (Exception ex)
        {
            throw new HttpRequestException("Failed to send request to CoinMarketCap API.", ex);
        }

        if (!response.IsSuccessStatusCode)
        {
            var errorBody = await response.Content.ReadAsStringAsync(cancellationToken);
            throw new HttpRequestException($"CoinMarketCap API returned error {response.StatusCode}: {errorBody}");
        }

        var apiResponse = await response.Content.ReadFromJsonAsync<CoinMarketCapApiResponse>(cancellationToken: cancellationToken);
        if (apiResponse is null)
            throw new InvalidOperationException("Failed to deserialize CoinMarketCap API response.");

        if (apiResponse.Status?.Error_Code != 0)
            throw new InvalidOperationException($"CoinMarketCap API error: {apiResponse.Status.Error_Message ?? "Unknown error"}");

        var upperCode = cryptoSymbol.ToUpperInvariant();
        if (!apiResponse.Data.TryGetValue(upperCode, out var cryptoData))
            throw new KeyNotFoundException($"Crypto code '{upperCode}' not found in CoinMarketCap response.");

        var usdQuote = cryptoData?.Quote?.USD;
        if (usdQuote is null)
            throw new InvalidOperationException($"USD quote for '{upperCode}' is not available.");

        return new CryptoPriceResult
        {
            PriceInUsd = usdQuote.Price
        };
    }

    public class CoinMarketCapApiResponse
    {
        public CoinMarketCapStatus Status { get; set; }

        public Dictionary<string, CoinMarketCapCryptoData> Data { get; set; }
    }

    public class CoinMarketCapStatus
    {
        public DateTime Timestamp { get; set; }

        public int Error_Code { get; set; }

        public string? Error_Message { get; set; }

        public int Elapsed { get; set; }

        public int Credit_Count { get; set; }

        public string? Notice { get; set; }
    }

    public class CoinMarketCapCryptoData
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public string Symbol { get; set; }

        public string Slug { get; set; }

        public int Num_Market_Pairs { get; set; }

        public DateTime Date_Added { get; set; }

        public List<string> Tags { get; set; }

        public decimal? Max_Supply { get; set; }

        public decimal Circulating_Supply { get; set; }

        public decimal Total_Supply { get; set; }

        public int Is_Active { get; set; }

        public bool Infinite_Supply { get; set; }

        public object? Platform { get; set; }

        public int Cmc_Rank { get; set; }

        public int Is_Fiat { get; set; }

        public decimal? Self_Reported_Circulating_Supply { get; set; }

        public decimal? Self_Reported_Market_Cap { get; set; }

        public decimal? Tvl_Ratio { get; set; }

        public DateTime Last_Updated { get; set; }

        public CoinMarketCapQuote Quote { get; set; }
    }

    public class CoinMarketCapQuote
    {
        public CoinMarketCapUsdQuote USD { get; set; }
    }

    public class CoinMarketCapUsdQuote
    {
        public decimal Price { get; set; }

        public decimal Volume_24h { get; set; }

        public decimal Volume_Change_24h { get; set; }

        public decimal Percent_Change_1h { get; set; }

        public decimal Percent_Change_24h { get; set; }

        public decimal Percent_Change_7d { get; set; }

        public decimal Percent_Change_30d { get; set; }

        public decimal Percent_Change_60d { get; set; }

        public decimal Percent_Change_90d { get; set; }

        public decimal Market_Cap { get; set; }

        public decimal Market_Cap_Dominance { get; set; }

        public decimal Fully_Diluted_Market_Cap { get; set; }

        public decimal? Tvl { get; set; }

        public DateTime Last_Updated { get; set; }
    }
}