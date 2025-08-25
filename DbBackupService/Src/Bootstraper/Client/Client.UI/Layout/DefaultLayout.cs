using MudBlazor;

namespace Client.UI.Layout;

public static class DefaultLayout
{
    public static readonly MudTheme Theme = new()
    {
        PaletteDark = new PaletteDark
        {
            Primary = "#fa892e",
            PrimaryLighten = "#ffa764",
            PrimaryDarken = "#f57a00",

            TextPrimary = "#FFFFFF",
            TextSecondary = "#E3E3E3",
            TextDisabled = "#7D7D7D",

            Dark = "#121212",
            DarkDarken = "#0A0A0A",
            DarkLighten = "#1C1C1C",

            Background = "#121212",
            AppbarBackground = "#1C1C1C",
            DrawerBackground = "#1C1C1C",

            Surface = "#1C1C1C"
        }
    };
}