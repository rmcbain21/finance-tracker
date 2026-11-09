namespace FinanceTracker.Core.Tests;

public class MoneyTests
{
    [Theory]
    [InlineData(84.37, "USD", "$84.37")]
    [InlineData(5, "USD", "$5.00")]
    [InlineData(1234.5, "USD", "$1,234.50")]
    [InlineData(84.37, "CAD", "CA$84.37")]
    [InlineData(-40, "USD", "−$40.00")]
    public void Money_shows_its_currency_symbol_and_two_decimals(decimal amount, string currency, string shown)
    {
        Assert.Equal(shown, Money.Of(amount, currency).ToString());
    }

    [Fact]
    public void Amounts_with_different_trailing_zeros_are_the_same_money()
    {
        Assert.Equal(Money.Of(12.5m, Money.Usd), Money.Of(12.50m, Money.Usd));
    }

    [Fact]
    public void The_same_amount_in_another_currency_is_different_money()
    {
        Assert.NotEqual(Money.Of(10m, Money.Usd), Money.Of(10m, Money.Cad));
    }

    [Fact]
    public void Only_known_currencies_are_allowed()
    {
        Assert.Throws<RuleViolationException>(() => Money.Of(10m, "EUR"));
    }

    [Fact]
    public void Fractions_of_a_cent_are_refused()
    {
        Assert.Throws<RuleViolationException>(() => Money.Of(3.459m, Money.Usd));
    }
}
