using CurrencyConverter.Middleware;
using CurrencyConverter.Model;
using CurrencyConverter.Services;
using CurrencyConverter.Services.Interfaces;

var builder = WebApplication.CreateBuilder(args);

// Add Configuration
builder.Configuration
    .AddJsonFile("Configuration/exchangeRates.json", optional: false, reloadOnChange: true)
    .AddEnvironmentVariables();

builder.Services.Configure<ExchangeRates>(builder.Configuration);

// Allow flat env vars like USD_TO_INR=81.00 to override JSON values
builder.Services.PostConfigure<ExchangeRates>(opts =>
{
    foreach (var key in opts.Rates.Keys.ToList())
    {
        var envVal = Environment.GetEnvironmentVariable(key);
        if (envVal is not null && decimal.TryParse(envVal, out var parsed))
            opts.Rates[key] = parsed;
    }
});

// Add services to the container.
builder.Services.AddScoped<ICurrencyConversionService, CurrencyConversionService>();
builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();
app.UseMiddleware<ExceptionMiddleware>();
// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
