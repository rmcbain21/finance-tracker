using System.Text.Json.Serialization;

namespace FinanceTracker.Core.Preferences;

/// <summary>Whether the app's light or dark colours are shown.</summary>
[JsonConverter(typeof(JsonStringEnumConverter<Appearance>))]
public enum Appearance
{
    /// <summary>Follows the device's dark mode setting.</summary>
    MatchDevice,

    Light,

    Dark,
}

/// <summary>The Owner's preferences. There is one Settings record, shared across devices by sync.</summary>
public sealed record Settings : StoredRecord
{
    public Appearance Appearance { get; init; } = Appearance.MatchDevice;

    /// <summary>The currency the app reports in by default: one of <see cref="Money.Currencies"/>.</summary>
    public string HomeCurrency { get; init; } = Money.Usd;
}
