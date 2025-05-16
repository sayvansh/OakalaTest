namespace Core.CryptoServices;

public interface ICryptoPriceService
{
    Task<CryptoPriceResult> GetCryptoPriceInUsdAsync(string cryptoSymbol, CancellationToken cancellationToken = default);
}

public sealed record CryptoPriceResult
{
    public decimal PriceInUsd { get; init; }
}