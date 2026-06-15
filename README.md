# Currency Converter API

An ASP.NET Core 8 Web API that converts amounts between USD, INR, and EUR using configurable exchange rates.

## Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)

## Getting Started

```bash
# Clone the repo, then from the repo root:
dotnet run
```

Swagger UI will open at `https://localhost:7151/swagger` (or `http://localhost:5138/swagger`).

## API

### `GET /convert`

Converts an amount from one currency to another.

| Parameter | Type | Description |
|---|---|---|
| `sourceCurrency` | string | ISO 4217 source currency code (e.g. `USD`) |
| `targetCurrency` | string | ISO 4217 target currency code (e.g. `INR`) |
| `amount` | decimal | Amount to convert (must be > 0) |

**Supported currencies:** `USD`, `INR`, `EUR`

**Example request:**
```
GET /convert?sourceCurrency=USD&targetCurrency=INR&amount=100
```

**Success response (200):**
```json
{
  "exchangeRate": 83.50,
  "convertedAmount": 8350.00
}
```

**Quick verification (PowerShell):**

```powershell
# Success
Invoke-RestMethod "http://localhost:5138/convert?sourceCurrency=USD&targetCurrency=INR&amount=100"

# Same currency
Invoke-RestMethod "http://localhost:5138/convert?sourceCurrency=USD&targetCurrency=USD&amount=100"

# Unsupported currency
Invoke-RestMethod "http://localhost:5138/convert?sourceCurrency=XYZ&targetCurrency=INR&amount=100"

# Missing parameter
Invoke-RestMethod "http://localhost:5138/convert?targetCurrency=INR&amount=100"

# Invalid amount
Invoke-RestMethod "http://localhost:5138/convert?sourceCurrency=USD&targetCurrency=INR&amount=-5"
```

**Error responses (400):**

| Scenario | Message |
|---|---|
| Missing `sourceCurrency` or `targetCurrency` | `sourceCurrency is required.` / `targetCurrency is required.` |
| Currency code not exactly 3 characters | `sourceCurrency must be a 3-letter ISO 4217 code.` |
| `amount` is zero or negative | `amount must be greater than zero.` |
| Source and target are the same | `Source and target currencies must be different.` |
| Unsupported currency pair | `Conversion rate from XYZ to INR not found.` |

All 400 responses use the same JSON envelope:
```json
{
  "message": "<description of the problem>"
}
```

## Configuration

Exchange rates are loaded from `Configuration/exchangeRates.json`:

```json
{
  "Rates": {
    "USD_TO_INR": 83.50,
    "INR_TO_USD": 0.012,
    "USD_TO_EUR": 0.92,
    "EUR_TO_USD": 1.09,
    "INR_TO_EUR": 0.011,
    "EUR_TO_INR": 90.50
  }
}
```

### Overriding rates with environment variables

Set a flat env var matching the rate key to override the JSON value — no restart needed:

```bash
# Windows (PowerShell)
$env:USD_TO_INR = "85.00"; dotnet run

# macOS / Linux
USD_TO_INR=85.00 dotnet run
```

### Dynamic reload (bonus)

Edit `Configuration/exchangeRates.json` while the app is running — the next request automatically picks up the new rates without restarting.

## Running Tests

```bash
dotnet test CurrencyConverter.Tests/CurrencyConverter.Tests.csproj
```

12 tests covering:
- Valid conversions and correct rounding
- Same-currency rejection
- Zero / negative amount rejection
- Unsupported currency pair rejection
- Lowercase currency code normalisation
- Rate override via `IOptionsMonitor`
- Controller 200 / 400 response mapping

## Project Structure

```
├── Configuration/exchangeRates.json        Exchange rate data
├── Controllers/CurrencyController.cs       GET /convert endpoint
├── Middleware/ExceptionMiddleware.cs        Global error handler
├── Model/                                  DTOs and config model
├── Services/                               Conversion logic + interface
├── CurrencyConverter.Tests/
│   ├── CurrencyConversionServiceTests.cs   Service unit tests
│   └── CurrencyControllerTests.cs          Controller unit tests
├── CurrencyConverter.slnx
└── README.md
```

## Architecture

### Request Flow

```
HTTP Request
    │
    ▼
ExceptionMiddleware          ← catches all unhandled exceptions
    │
    ▼
CurrencyController           ← validates input via DataAnnotations ([ApiController])
    │                           logs incoming request
    ▼
CurrencyConversionService    ← domain guards (same currency, amount > 0)
    │                           looks up rate, rounds result, logs conversion
    ▼
IOptionsMonitor<ExchangeRates>  ← always reads latest rates (supports hot-reload)
    │
    ▼
HTTP Response
```

### Configuration Pipeline

Exchange rates are resolved in this order (later sources win):

```
exchangeRates.json
    └─▶ Environment variables (flat key, e.g. USD_TO_INR=85.00)
            └─▶ PostConfigure override (applied last, highest precedence)
```

### Error Handling

```
ArgumentException  ──▶  ExceptionMiddleware  ──▶  400 Bad Request
Any other exception ──▶  ExceptionMiddleware  ──▶  500 Internal Server Error
Invalid query params ──▶ [ApiController] auto ──▶  400 ValidationProblemDetails
```

### Layers

| Layer | Responsibility |
|---|---|
| `Controllers/` | HTTP binding, input validation, logging |
| `Services/` | Domain logic, rate lookup, conversion |
| `Middleware/` | Cross-cutting error handling |
| `Model/` | DTOs and configuration models |
| `Configuration/` | Exchange rate data file |

## Design Decisions

- **`IOptionsMonitor` over `IOptions`**: `CurrentValue` is re-read on every call, so file changes are picked up instantly — this is how dynamic reload works with zero extra code.
- **`PostConfigure` for env var overrides**: The flat key format (`USD_TO_INR`) required by the spec isn't natively supported by ASP.NET Core's configuration pipeline (which uses double-underscore separators). A `PostConfigure` action reads env vars by key after JSON binding, giving the correct precedence.
- **Middleware owns all error mapping**: `ExceptionMiddleware` catches `ArgumentException` → 400 and everything else → 500. The controller stays clean — no try/catch, just service call and `Ok`. This avoids duplicating error-handling logic across controllers as the API grows.
