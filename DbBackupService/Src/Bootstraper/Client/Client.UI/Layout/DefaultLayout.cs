using MudBlazor;

namespace Client.UI.Layout;

public static class DefaultLayout
{
    private const string DarkColor = "#0D0D0D"; // Background
    private const string SurfaceColor = "#141414";

    public static readonly MudTheme Theme = new()
    {
        PaletteDark = new PaletteDark
        {
            Primary = "#F24236",
            PrimaryLighten = "#F06056",
            PrimaryDarken = "#AD1B13",

            Secondary = "#6C8EAD",
            SecondaryDarken = "#3A5D85",
            SecondaryLighten = "#91C9FF",
            SecondaryContrastText = "#000",

            TextPrimary = "#F7F7F7",
            TextSecondary = "#B0B0B0",
            TextDisabled = "#474747",

            Dark = DarkColor,
            DarkDarken = "#000",
            DarkLighten = "#303030",

            Background = DarkColor,
            AppbarBackground = SurfaceColor,
            DrawerBackground = SurfaceColor,

            Surface = SurfaceColor
        }
    };

    public static PaletteDark ThemePalette => Theme.PaletteDark;
}