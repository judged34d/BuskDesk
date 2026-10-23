namespace BuskDesk.Core;

public enum ChannelRole
{
    Raw,
    Intensity,
    Pan,
    Tilt,
    PanFine,
    TiltFine,
    ColorRed,
    ColorGreen,
    ColorBlue,
    ColorWhite,
    ColorAmber,
    ColorUv,
    ColorWheel,
    Gobo,
    Strobe,
    Shutter,
    Speed,
    Focus,
    Zoom,
    Prism,
    Gate,
    Mode,
    Reset,
    Protected
}

public static class ChannelRoleExtensions
{
    public static bool IsIntensityFamily(this ChannelRole role) =>
        role is ChannelRole.Intensity
            or ChannelRole.ColorRed
            or ChannelRole.ColorGreen
            or ChannelRole.ColorBlue
            or ChannelRole.ColorWhite
            or ChannelRole.ColorAmber
            or ChannelRole.ColorUv;

    public static bool IsProtected(this ChannelRole role) =>
        role is ChannelRole.Reset or ChannelRole.Protected or ChannelRole.Gate or ChannelRole.Mode;

    public static string DisplayName(this ChannelRole role) => role switch
    {
        ChannelRole.Intensity => "Dimmer",
        ChannelRole.Pan => "Pan",
        ChannelRole.Tilt => "Tilt",
        ChannelRole.PanFine => "Pan Fine",
        ChannelRole.TiltFine => "Tilt Fine",
        ChannelRole.ColorRed => "Rot",
        ChannelRole.ColorGreen => "Grün",
        ChannelRole.ColorBlue => "Blau",
        ChannelRole.ColorWhite => "Weiß",
        ChannelRole.ColorAmber => "Amber",
        ChannelRole.ColorUv => "UV",
        ChannelRole.ColorWheel => "Farbrad",
        ChannelRole.Gobo => "Gobo",
        ChannelRole.Strobe => "Strobe",
        ChannelRole.Shutter => "Shutter",
        ChannelRole.Speed => "Tempo",
        ChannelRole.Focus => "Focus",
        ChannelRole.Zoom => "Zoom",
        ChannelRole.Prism => "Prisma",
        ChannelRole.Gate => "Tor / Betrieb",
        ChannelRole.Mode => "Modus",
        ChannelRole.Reset => "Reset",
        ChannelRole.Protected => "Schutz",
        _ => "Roh"
    };
}
