namespace BuskDesk.Model;

/// <summary>Named focus point for one patched fixture (pan/tilt).</summary>
public sealed class FocusPoint
{
    public string Id { get; set; } = Guid.NewGuid().ToString("N");
    public string Name { get; set; } = "Position";
    public string FixtureInstanceId { get; set; } = string.Empty;
    public byte Pan { get; set; }
    public byte Tilt { get; set; }
    public byte? PanFine { get; set; }
    public byte? TiltFine { get; set; }
}

/// <summary>
/// Lean A↔B movement (inspired by small-desk workflows / Licon-style motion presets).
/// Official Licon 1 offers geometric patterns; Busk Desk stores user A→B paths with optional look.
/// </summary>
public sealed class MovePattern
{
    public string Id { get; set; } = Guid.NewGuid().ToString("N");
    public string Name { get; set; } = "Pattern";
    public string FixtureInstanceId { get; set; } = string.Empty;

    public byte PanA { get; set; }
    public byte TiltA { get; set; }
    public byte? PanFineA { get; set; }
    public byte? TiltFineA { get; set; }

    public byte PanB { get; set; }
    public byte TiltB { get; set; }
    public byte? PanFineB { get; set; }
    public byte? TiltFineB { get; set; }

    /// <summary>Seconds for one way A→B (return B→A uses the same).</summary>
    public double OneWaySeconds { get; set; } = 3.0;

    public bool IncludeIntensity { get; set; }
    public bool IncludeColor { get; set; }
    public bool IncludeGobo { get; set; }
    public bool IncludeStrobe { get; set; }

    public byte Intensity { get; set; } = 255;
    public byte Red { get; set; }
    public byte Green { get; set; }
    public byte Blue { get; set; }
    public byte White { get; set; }
    public byte Gobo { get; set; }
    public byte Strobe { get; set; }
}
