using CurrencyConverter.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace CurrencyConverter.Controllers
{
    [ApiController]
    [Route("convert")]
    public class CurrencyController : ControllerBase
    {
        private readonly ICurrencyConversionService _conversionService;
        private readonly ILogger<CurrencyController> _logger;

        public CurrencyController(ICurrencyConversionService conversionService, ILogger<CurrencyController> logger)
        {
            _conversionService = conversionService;
            _logger = logger;
        }

        [HttpGet]
        public IActionResult Convert(string sourceCurrency, string targetCurrency, decimal amount)
        {
            _logger.LogInformation("Received conversion request: {Amount} from {Source} to {Target}",
                amount, sourceCurrency, targetCurrency);

            // Call the conversion service to perform the currency conversion
            var result = _conversionService.Convert(sourceCurrency, targetCurrency, amount);

            _logger.LogInformation("Conversion result: {Amount} from {Source} to {Target} is {Result}",
                amount, sourceCurrency, targetCurrency, result.ConvertedAmount);
            // Return the conversion result as a JSON response
            return Ok(result);

        }
    }
}
