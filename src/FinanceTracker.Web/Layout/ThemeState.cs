using FinanceTracker.Core.Preferences;

namespace FinanceTracker.Web.Layout;

/// <summary>
/// The Appearance in use, shared so that a change made in Settings shows straight away:
/// the Settings page saves the choice and sets it here, and MainLayout applies it.
/// </summary>
public sealed class ThemeState
{
    public Appearance Appearance { get; private set; }

    public event Action? Changed;

    public void Set(Appearance appearance)
    {
        if (appearance != Appearance)
        {
            Appearance = appearance;
            Changed?.Invoke();
        }
    }
}
