# Busk Desk — Architektur (Stand Mockup)

## Module (geplant)

| Ordner / Projekt | Aufgabe |
| --- | --- |
| `src/BuskDesk` | UI (Avalonia) + vorerst Mock-ViewModels |
| Engine (später) | DMX-Puffer, Merge, 40 Hz |
| Output (später) | Art-Net, Enttec Pro |
| Probe (später) | Sonde → Fixture-JSON |
| Audio (später) | WASAPI Loopback, Live-Feinschliff |
| Listen (später) | Song-Analyse inkl. Mood-Modelle, Bibliothek |
| Spotify (später) | Track-ID / Now Playing |

## Daten

- Entwicklung: Ordner `fixtures/`, `shows/`, `library/`, `models/` im Repo-Root
- Installation (später): App unter Program Files, Nutzerdaten unter `%AppData%\BuskDesk`

## Auslieferung

1. Jetzt: `scripts/run-mock.ps1` bzw. `publish/mock/BuskDesk.exe`
2. Später: Windows-Setup (Inno Setup / MSIX) inkl. Modelle
