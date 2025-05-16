namespace OkalaTest.Crypto.Price.List;

public class GetCryptoRatesResponse
{
    public string CryptoCode { get; set; } = string.Empty;
    
    public Dictionary<string, decimal> Prices { get; set; } = new();
}