namespace Core.ExchangeServices;

public interface IExchangeRatesService
{
    Task<ExchangeRatesResponse> GetHistoricalRatesAsync(
        string date,
        string baseCurrency,
        IEnumerable<string> symbols,
        CancellationToken cancellationToken = default);
}

public sealed record ExchangeRatesResponse
{
    public bool Success { get; set; }

    public long Timestamp { get; set; }

    public string Base { get; set; }

    public string Date { get; set; }

    public Dictionary<string, decimal> Rates { get; set; }
}