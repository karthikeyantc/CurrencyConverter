using CurrencyConverter.Model;
using CurrencyConverter.Services.Interfaces;
using Microsoft.Extensions.Options;

namespace CurrencyConverter.Services
{
    public class CurrencyConversionService : ICurrencyConversionService
    {
        private readonly IOptionsMonitor<ExchangeRates> _exchangeRates;
        private readonly ILogger<CurrencyConversionService> _logger;

        public CurrencyConversionService(IOptionsMonitor<ExchangeRates> exchangeRates, ILogger<CurrencyConversionService> logger)
        {
            _exchangeRates = exchangeRates;
            _logger = logger;
        }

        public ConvertResponse Convert(string source, string target, decimal amount)
        {
            if (amount <= 0)
            {
                throw new ArgumentException("Amount must be greater than zero");
            }

            if (source.ToUpper() == target.ToUpper())
            {
                throw new ArgumentException("Source and target currencies must be different.");
            }

            var key = $"{source.ToUpper()}_TO_{target.ToUpper()}";

            var rates = _exchangeRates.CurrentValue.Rates;
            if (!rates.TryGetValue(key, out var rate))
            {
                throw new ArgumentException($"Conversion rate from {source} to {target} not found.");
            }

            var convertedAmount = Math.Round(amount * rate, 2);
            _logger.LogInformation("Converted {Amount} from {Source} to {ConvertedAmount} {Target} at rate {Rate}",
                amount, source, convertedAmount, target, rate);

            return new ConvertResponse
            {
                ExchangeRate = rate,
                ConvertedAmount = convertedAmount
            };
        }
    }
}