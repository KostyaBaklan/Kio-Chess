namespace Analysis.Core.Models;

public class ThemeInfo
{
    public string Key { get; init; } = string.Empty;
    public string DisplayName { get; init; } = string.Empty;
    public string ResourcePath { get; init; } = string.Empty;

    public override string ToString() => DisplayName;

    public static readonly IReadOnlyList<ThemeInfo> All =
    [
        new() { Key = "ClassicWood",   DisplayName = "Classic Wood",   ResourcePath = "Themes/ClassicWood/Theme.xaml" },
        new() { Key = "MidnightBlue",  DisplayName = "Midnight Blue",  ResourcePath = "Themes/MidnightBlue/Theme.xaml" },
        new() { Key = "Arctic",        DisplayName = "Arctic",         ResourcePath = "Themes/Arctic/Theme.xaml" },
        new() { Key = "Emerald",       DisplayName = "Emerald",        ResourcePath = "Themes/Emerald/Theme.xaml" },
        new() { Key = "Crimson",       DisplayName = "Crimson",        ResourcePath = "Themes/Crimson/Theme.xaml" },
        new() { Key = "NeonCyber",     DisplayName = "Neon Cyber",     ResourcePath = "Themes/NeonCyber/Theme.xaml" },
        new() { Key = "RoseGold",      DisplayName = "Rose Gold",      ResourcePath = "Themes/RoseGold/Theme.xaml" },
        new() { Key = "SlatePro",      DisplayName = "Slate Pro",      ResourcePath = "Themes/SlatePro/Theme.xaml" },
        new() { Key = "Parchment",     DisplayName = "Parchment",      ResourcePath = "Themes/Parchment/Theme.xaml" },
        new() { Key = "HighContrast",  DisplayName = "High Contrast",  ResourcePath = "Themes/HighContrast/Theme.xaml" },
    ];
}
