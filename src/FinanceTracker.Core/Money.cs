using System.Globalization;

namespace FinanceTracker.Core;

/// <summary>
/// An amount in the currency it was paid in. Amounts are never bare numbers and are never
/// converted between currencies (ADR 0002).
/// </summary>
/// <param name="Amount">In the currency's main unit, such as dollars, to the cent.</param>
/// <param name="Currency">An ISO 4217 code: <see cref="Usd"/> or <see cref="Cad"/>.</param>
public readonly record struct Money(decimal Amount, string Currency)
{
    public const string Usd = "USD";
    public const string Cad = "CAD";

    /// <summary>The currencies the app knows, for the US and Canada.</summary>
    public static readonly IReadOnlyList<string> Currencies = [Usd, Cad];

    /// <summary>Checks the amount and currency and returns the Money.</summary>
    /// <exception cref="RuleViolationException">The currency is unknown, or the amount has fractions of a cent.</exception>
    public static Money Of(decimal amount, string currency)
    {
        if (!Currencies.Contains(currency))
        {
            throw new RuleViolationException($"\"{currency}\" isn't a currency the app knows.");
        }

        if (decimal.Round(amount, 2) != amount)
        {
            throw new RuleViolationException("Amounts can't have fractions of a cent.");
        }

        return new Money(amount, currency);
    }

    /// <summary>Such as "$1,234.56" for USD and "CA$1,234.56" for CAD, with a leading minus for refunds.</summary>
    public override string ToString()
    {
        var symbol = Currency switch
        {
            Usd => "$",
            Cad => "CA$",
            _ => Currency + " ",
        };
        var digits = Math.Abs(Amount).ToString("#,##0.00", CultureInfo.InvariantCulture);
        return Amount < 0 ? $"−{symbol}{digits}" : $"{symbol}{digits}";
    }
}
