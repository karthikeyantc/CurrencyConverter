using CurrencyConverter.Model;

namespace CurrencyConverter.Services.Interfaces
{
    public interface ICurrencyConversionService
    {
        ConvertResponse Convert(
        string source,
        string target,
        decimal amount);
    }
}