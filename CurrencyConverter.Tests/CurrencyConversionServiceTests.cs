using CurrencyConverter.Model;
using CurrencyConverter.Services;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;

namespace CurrencyConverter.Tests
{
    public class CurrencyConversionServiceTests
    {
        private static CurrencyConversionService BuildService(Dictionary<string, decimal> rates)
        {
            var opts = new ExchangeRates { Rates = rates };
            var monitor = new Mock<IOptionsMonitor<ExchangeRates>>();
            monitor.Setup(m => m.CurrentValue).Returns(opts);
            var logger = Mock.Of<ILogger<CurrencyConversionService>>();
            return new CurrencyConversionService(monitor.Object, logger);
        }

        private static Dictionary<string, decimal> DefaultRates() => new()
        {
            ["USD_TO_INR"] = 83.50m,
            ["INR_TO_USD"] = 0.012m,
            ["USD_TO_EUR"] = 0.92m,
            ["EUR_TO_USD"] = 1.09m,
            ["INR_TO_EUR"] = 0.011m,
            ["EUR_TO_INR"] = 90.50m,
        };

        [Fact]
        public void Convert_ValidPair_ReturnsCorrectRateAndAmount()
        {
            var sut = BuildService(DefaultRates());

            var result = sut.Convert("USD", "INR", 100m);

            result.ExchangeRate.Should().Be(83.50m);
            result.ConvertedAmount.Should().Be(8350.00m);
        }

        [Fact]
        public void Convert_RoundsResultToTwoDecimalPlaces()
        {
            var sut = BuildService(new Dictionary<string, decimal> { ["USD_TO_INR"] = 83.333m });

            var result = sut.Convert("USD", "INR", 10m);

            result.ConvertedAmount.Should().Be(833.33m);
        }

        [Fact]
        public void Convert_LowercaseInput_NormalisesAndSucceeds()
        {
            var sut = BuildService(DefaultRates());

            var result = sut.Convert("usd", "inr", 1m);

            result.ExchangeRate.Should().Be(83.50m);
        }

        [Fact]
        public void Convert_SameCurrency_ThrowsArgumentException()
        {
            var sut = BuildService(DefaultRates());

            var act = () => sut.Convert("USD", "USD", 100m);

            act.Should().Throw<ArgumentException>()
               .WithMessage("*must be different*");
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        [InlineData(-100)]
        public void Convert_ZeroOrNegativeAmount_ThrowsArgumentException(decimal amount)
        {
            var sut = BuildService(DefaultRates());

            var act = () => sut.Convert("USD", "INR", amount);

            act.Should().Throw<ArgumentException>()
               .WithMessage("*greater than zero*");
        }

        [Fact]
        public void Convert_UnsupportedPair_ThrowsArgumentException()
        {
            var sut = BuildService(DefaultRates());

            var act = () => sut.Convert("XYZ", "INR", 100m);

            act.Should().Throw<ArgumentException>()
               .WithMessage("*not found*");
        }

        [Fact]
        public void Convert_OverriddenRate_UsesNewRate()
        {
            var rates = new Dictionary<string, decimal> { ["USD_TO_INR"] = 83.50m };
            var opts = new ExchangeRates { Rates = rates };
            var monitor = new Mock<IOptionsMonitor<ExchangeRates>>();

            // Simulate a hot-reload: CurrentValue returns updated rates
            monitor.Setup(m => m.CurrentValue).Returns(new ExchangeRates
            {
                Rates = new Dictionary<string, decimal> { ["USD_TO_INR"] = 85.00m }
            });

            var sut = new CurrencyConversionService(monitor.Object, Mock.Of<ILogger<CurrencyConversionService>>());

            var result = sut.Convert("USD", "INR", 100m);

            result.ExchangeRate.Should().Be(85.00m);
            result.ConvertedAmount.Should().Be(8500.00m);
        }
    }
}
