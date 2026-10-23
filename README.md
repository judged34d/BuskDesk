# Busk Desk

Modernes Windows-Lichtpult für kleine Shows: Sonde, Patch, Zwei-Preset-Pult, Szenen, Air2DMX / Art-Net / USB-DMX-Pro.

**Status:** v0.3 — Fokuspunkte (Pfeiltasten) + Bewegung A↔B  
**Stack:** .NET 8 · Avalonia 11

> **Arbeitskopie:** Ab sofort nur noch auf dem **Laptop** entwickeln (siehe [docs/LAPTOP.md](docs/LAPTOP.md)).

## Schnellstart (Laptop)

```powershell
git clone https://github.com/judged34d/BuskDesk.git
cd BuskDesk
.\scripts\run.ps1
```

Self-contained Build (Runtime eingebettet):

```powershell
.\scripts\publish-standalone.ps1
.\publish\win-x64\BuskDesk.exe
```

## Erster Test: U'King Spot + Air2DMX

1. Spot: DMX-Adresse **001**, gewünschter Kanalmodus  
2. Laptop → WLAN `JMS_AIR2DMX_…`  
3. Busk Desk → **Ausgang** → **Air2DMX** → Verbinden  
4. **Sonde**: Kanäle ausmessen, Rollen setzen, als Fixture speichern  
5. **Patch**: gespeichertes Profil auf die Startadresse legen, dann am **Pult** fahren  

Keine vorgefertigten Hersteller-Profile — genau der Fall, wenn online nichts Brauchbares existiert.

## Funktionen

| Bereich | Inhalt |
| --- | --- |
| Ausgang | Simulation, Air2DMX, Art-Net, Enttec Pro / USB2DMX |
| Engine | 40 Hz, Merge, Grand Master, Blackout |
| Sonde | Live-Kanäle → Fixture JSON |
| Patch / Pult / Szenen | Speichern, Crossfade, Submaster, GO |
| **Move** | Pfeiltasten Pan/Tilt, Fokuspunkte, Pattern A↔B inkl. optional Farbe/Gobo/Dimmer/Strobe |

Platzhalter: Assistent, Titel-Bibliothek. Siehe [docs/licon-move.md](docs/licon-move.md).

## Docs

- [Laptop-Setup & Test](docs/LAPTOP.md)
- [Ausgänge / Air2DMX](docs/outputs.md)
- [Architektur](docs/architecture.md)
