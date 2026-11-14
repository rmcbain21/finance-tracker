using MudBlazor;

namespace FinanceTracker.Web.Layout;

/// <summary>
/// The app's colours, light and dark. Settings → Appearance picks which (MainLayout applies it).
/// </summary>
public static class AppTheme
{
    public static readonly MudTheme Theme = new()
    {
        PaletteLight = new PaletteLight
        {
            Primary = "#2E6B4F", PrimaryDarken = "#24543E", PrimaryLighten = "#4A8A6C",
            Secondary = "#B7791F", SecondaryDarken = "#925F15", SecondaryLighten = "#D4983F", SecondaryContrastText = "#FFFFFF",
            Background = "#F4F6F3", Surface = "#FFFFFF",
            AppbarBackground = "#24543E", AppbarText = "#F1F8F4",
            DrawerBackground = "#E8EEEA", DrawerText = "#24543E", DrawerIcon = "#24543E",
            TextPrimary = "#1D2621", TextSecondary = "#5B6760", ActionDefault = "#5B6760", Divider = "#D8E0DA",
        },
        PaletteDark = new PaletteDark
        {
            Primary = "#7CC9A3", PrimaryDarken = "#5FB087", PrimaryLighten = "#A4DCC0", PrimaryContrastText = "#0B2418",
            Secondary = "#EDB45E", SecondaryContrastText = "#2A1A02",
            Background = "#111613", Surface = "#1A211D",
            AppbarBackground = "#141A16", AppbarText = "#E2EFE7",
            DrawerBackground = "#151B17", DrawerText = "#A4DCC0", DrawerIcon = "#A4DCC0",
            TextPrimary = "#E3EAE5", TextSecondary = "#9DAAA2", ActionDefault = "#9DAAA2", Divider = "#2B3530",
        },
    };
}
