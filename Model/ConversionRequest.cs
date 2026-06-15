using System.ComponentModel.DataAnnotations;

namespace CurrencyConverter.Model
{
    public class ConversionRequest
    {
        [Required(ErrorMessage = "sourceCurrency is required.")]
        [StringLength(3, MinimumLength = 3, ErrorMessage = "sourceCurrency must be a 3-letter ISO 4217 code.")]
        public string SourceCurrency { get; set; } = string.Empty;

        [Required(ErrorMessage = "targetCurrency is required.")]
        [StringLength(3, MinimumLength = 3, ErrorMessage = "targetCurrency must be a 3-letter ISO 4217 code.")]
        public string TargetCurrency { get; set; } = string.Empty;

        [Range(0.0001, double.MaxValue, ErrorMessage = "amount must be greater than zero.")]
        public decimal Amount { get; set; }
    }
}
