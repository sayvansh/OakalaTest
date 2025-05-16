using Core.CryptoServices;
using FastEndpoints;
using FluentValidation;

namespace OkalaTest.Crypto.Price.List;

file sealed class Endpoint : Endpoint<GetCryptoRatesRequest,GetCryptoRatesResponse>
{
    private readonly ICryptoQuoteService _exchangeRatesService;

    public Endpoint(ICryptoQuoteService exchangeRatesService)
    {
        _exchangeRatesService = exchangeRatesService;
    }


    public override void Configure()
    {
        Get("crypto/{symbol}/quotes");
        AllowAnonymous();
        Version(1);
    }

    public override async Task HandleAsync(GetCryptoRatesRequest req, CancellationToken ct)
    {
        var response = await _exchangeRatesService.GetCryptoPricesAsync("BTC", ct);
        await SendOkAsync(new GetCryptoRatesResponse()
        {
            CryptoCode = response.CryptoCode,
            Prices = response.Prices
        }, ct);
    }
}

file sealed class EndpointSummary : Summary<Endpoint>
{
    public EndpointSummary()
    {
        Summary = "Gets the latest quote for a given cryptocurrency and returns its value in specified target fiat currencies.";
        Description = "Gets the latest quote for a given cryptocurrency and returns its value in specified target fiat currencies.";
        Response<GetCryptoRatesResponse>(200, "Success");
    }
}

file sealed class RequestValidator : Validator<GetCryptoRatesRequest>
{
    public RequestValidator()
    {
        RuleFor(request => request.Symbol)
            .NotEmpty().WithMessage("Enter Valid Symbol")
            .NotNull().WithMessage("Enter Symbol");
    }
}