using MudBlazor;

namespace Client.UI.Layout;

public static class DefaultLayout
{
    public static PaletteDark ThemePalette => Theme.PaletteDark;
    public static readonly MudTheme Theme = new()
    {
        PaletteDark = new PaletteDark
        {
            Primary = "#00d9ff",
            PrimaryLighten = "#8AE8FF",
            PrimaryDarken = "#00A2BA",

            TextPrimary = "#FFFFFF",
            TextSecondary = "#E3E3E3",
            TextDisabled = "#7D7D7D",

            Dark = "#171717",
            DarkDarken = "#121212",
            DarkLighten = "#1C1C1C",

            Background = "#171717",
            AppbarBackground = "#1F1E1E",
            DrawerBackground = "#1F1E1E",

            Surface = "#1C1C1C"
        }
    };
}