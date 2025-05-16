using Core.ExchangeServices;

namespace Core.CryptoServices;

public interface ICryptoQuoteService
{
    Task<CryptoQuoteResult> GetCryptoPricesAsync(string cryptoSymbol, CancellationToken cancellationToken = default);
}

public record CryptoQuoteResult
{
    public string CryptoCode { get; init; } = string.Empty;
    
    public Dictionary<string, decimal> Prices { get; init; } = new();
}