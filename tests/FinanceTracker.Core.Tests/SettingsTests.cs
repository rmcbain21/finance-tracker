using FinanceTracker.Core.Preferences;
using FinanceTracker.Core.Tests.Fakes;

namespace FinanceTracker.Core.Tests;

public class SettingsTests
{
    private readonly InMemoryRecordStore _store = new();
    private readonly FakeClock _clock = new(new DateTimeOffset(2026, 11, 9, 18, 0, 0, TimeSpan.Zero));

    private AppCore CreateApp() => new(_store, _clock);

    [Fact]
    public async Task Defaults_are_matching_the_device_in_us_dollars()
    {
        var settings = await CreateApp().Settings.GetAsync();

        Assert.Equal((Appearance.MatchDevice, Money.Usd), (settings.Appearance, settings.HomeCurrency));
    }

    [Fact]
    public async Task Appearance_and_home_currency_are_saved()
    {
        var app = CreateApp();

        await app.Settings.SetAppearanceAsync(Appearance.Dark);
        await app.Settings.SetHomeCurrencyAsync(Money.Cad);

        var settings = await CreateApp().Settings.GetAsync();
        Assert.Equal((Appearance.Dark, Money.Cad), (settings.Appearance, settings.HomeCurrency));
        Assert.Equal(_clock.UtcNow, settings.UpdatedAt);
    }

    [Fact]
    public async Task There_is_only_one_settings_record_with_a_fixed_id_so_every_device_edits_the_same_one()
    {
        var app = CreateApp();
        await app.Settings.SetAppearanceAsync(Appearance.Light);
        await app.Settings.SetHomeCurrencyAsync(Money.Cad);

        var settings = Assert.Single(await _store.ListAsync<Settings>("settings"));

        Assert.Equal("00000000-0000-0000-0000-000000000001", settings.Id);
    }

    [Fact]
    public async Task Unknown_choices_are_refused()
    {
        var app = CreateApp();

        await Assert.ThrowsAsync<RuleViolationException>(() => app.Settings.SetAppearanceAsync((Appearance)99));
        await Assert.ThrowsAsync<RuleViolationException>(() => app.Settings.SetHomeCurrencyAsync("EUR"));
    }
}
