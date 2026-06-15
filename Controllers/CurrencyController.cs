using CurrencyConverter.Model;
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
        public IActionResult Convert([FromQuery] ConversionRequest request)
        {
            _logger.LogInformation("Received conversion request: {Amount} from {Source} to {Target}",
                request.Amount, request.SourceCurrency, request.TargetCurrency);

            var result = _conversionService.Convert(request.SourceCurrency, request.TargetCurrency, request.Amount);

            _logger.LogInformation("Conversion result: {Amount} from {Source} to {Target} is {Result}",
                request.Amount, request.SourceCurrency, request.TargetCurrency, result.ConvertedAmount);

            return Ok(result);
        }
    }
}
