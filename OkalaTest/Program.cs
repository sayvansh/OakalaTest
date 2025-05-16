using System.Net.Http.Headers;
using System.Text.Json;
using Core.CryptoServices;
using Core.ExchangeServices;
using FastEndpoints;
using FastEndpoints.Swagger;
using OkalaTest;
using OkalaTest.CoinMarketCapCryptoServices;
using OkalaTest.Crypto;
using OkalaTest.ExchangeRatesServices;

var builder = WebApplication.CreateBuilder(args);
builder.WebHost.UseKestrel();
builder.WebHost.ConfigureKestrel((_, options) =>
{
    options.ListenAnyIP(7050, _ => { });
    // options.ListenAnyIP(7051, listenOptions =>
    // {
    //     listenOptions.Protocols = HttpProtocols.Http1AndHttp2AndHttp3;
    //     listenOptions.UseHttps();
    // });
});
builder.Services.AddHealthChecks();
builder.Services.AddCors();
builder.Services.AddFastEndpoints();

builder.Services.SwaggerDocument(settings =>
{
    settings.DocumentSettings = generatorSettings =>
    {
        generatorSettings.Title = "Crypto - WebApi";
        generatorSettings.DocumentName = "v1";
        generatorSettings.Version = "v1";
    };
    settings.EnableJWTBearerAuth = false;
    settings.MaxEndpointVersion = 1;
});

builder.Services.AddHttpClient("CoinMarketCap", c =>
{
    c.BaseAddress = new Uri("https://pro-api.coinmarketcap.com/v1/");
    c.DefaultRequestHeaders
        .Accept
        .Add(new MediaTypeWithQualityHeaderValue("application/json"));
});

builder.Services.AddHttpClient("ExchangeRate", c =>
{
    c.BaseAddress = new Uri("https://api.exchangeratesapi.io/v1/");
    c.DefaultRequestHeaders
        .Accept
        .Add(new MediaTypeWithQualityHeaderValue("application/json"));
});

builder.Services.AddScoped<ICryptoPriceService, CoinMarketCapCryptoPriceService>();
builder.Services.AddScoped<IExchangeRatesService, ExchangeRatesService>();
builder.Services.AddScoped<ICryptoQuoteService, CryptoQuoteService>();


var app = builder.Build();

app.UseCors(b => b.AllowAnyHeader()
    .AllowAnyMethod()
    .SetIsOriginAllowed(_ => true)
    .AllowCredentials());

app.UseHealthChecks("/health");
app.UseFastEndpoints(config =>
{
    config.Serializer.Options.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
    config.Endpoints.RoutePrefix = "api";
    config.Versioning.Prefix = "v";
    config.Versioning.PrependToRoute = true;
});


// if (app.Environment.IsDevelopment())
// {
app.UseOpenApi();
app.UseSwaggerUi(s => s.ConfigureDefaults());
// }

await app.RunAsync();