# Busk Desk — Chat-Zusammenfassung (Handoff)

Stand: 23. Oktober 2026 · Repo: https://github.com/judged34d/BuskDesk · Version im Code: **v0.3 Move**

Dieser Text fasst die Planungs- und Entwicklungsgespräche zusammen, damit die Arbeit auf dem **Laptop** weitergeht. Am bisherigen PC soll **nicht** parallel weiterentwickelt werden (Versionskonflikte vermeiden).

---

## 1. Produktziel

**Busk Desk** = kostenlose, moderne Windows-DMX-Software für **ganz kleine bis kleine Shows**.

Zielgruppe: Wer mehr will als Autoprogramme am Scheinwerfer (Partyraum, kleine Bühne), aber kein grandMA/MA2 und keine teure Fixture-Bibliothek braucht.

Kernprobleme der freien Konkurrenz (QLC+, DMXControl 3, Freestyler):

- Fixture-Profile für No-Name/U'King oft fehlend oder schlecht
- Fixture-Editor umständlich (tippen statt an der Lampe lernen)
- UI wirkt veraltet („Windows 95“)
- Kein schlankes Zwei-Preset-Pult als Einstieg
- Kein Assistent, der *eigene* Fixtures kennt

**Nicht-Ziele (bewusst schlank):** 3D-Visualizer, Pixelmapping, Timecode/MSC, Multi-Operator, Webcam-Auto-Profile, Tournee-Tracking-Shows.

---

## 2. Name & Auslieferung

- Produktname: **Busk Desk** (Alltag: „Busk“, Exe: `BuskDesk.exe`)
- „Busk“ = live am Pult improvisieren; „Desk“ = Pult/Software
- Später: Windows-**Setup**-Installer (nicht nur portable ZIP)
- Entwicklung: Git; Daten unter `fixtures/`, `shows/`, `library/`, `models/`

---

## 3. Funktionsentscheidungen (schlank)

### Rein / geplant

| Thema | Entscheidung |
| --- | --- |
| Fixture anlegen | **Sonde**: Kanäle live fahren, Reaktion sehen, Rolle + **Wertbereiche** setzen (z. B. 0–120 Rot, 121–150 Blau), speichern |
| Keine Preset-Bibliothek | Keine mitgelieferten U'King-JSON-Profile — Profile entstehen am Gerät |
| Pult | Zwei Presets A/B, Crossfade, Submaster, Grand Master, Blackout |
| Szenen | Look speichern / GO |
| Patch | Profile auf Startadressen |
| Ausgang | Plugin-Architektur `IDmxOutput` |
| Air2DMX | **WLAN-Art-Net-Node** (USB nur Strom) → Art-Net an `2.0.0.1` |
| USB2DMX / Enttec Pro | Enttec-Pro-API über COM |
| Fokuspunkte | Pfeiltasten Pan/Tilt, benannte Positionen (Rednerpult, Seitenwand) |
| Bewegung | **A↔B-Pattern** (nicht Licon-Kreise): Mark A, Mark B, optional Dimmer/Farbe/Gobo/Strobe, Play/Stop ping-pong |
| Audio / Assistent | Später: Live WASAPI zuerst; Listen-Modus für Titel; Spotify = Track-ID |
| Stimmung-KI | Später, eingebettet in Installer; schwer in Listen, leicht am Gig |
| Webcam-Auto-Profile | **Abgelehnt** — bleibt manuell |

### Licon 1 (Recherche)

Offizielles Handbuch: vorprogrammierte Moving-Patterns (Kreis, Lissajous, Schwenks, Size/Speed/Phase).  
Nutzer-Workflow vom alten Pult: Start/Ende mit Hand, speichern, Taste = hin und her + Look.  
→ Busk Desk setzt den **Nutzer-Workflow** um (praktischer für kleine Bühnen). Siehe `docs/licon-move.md`.

### Merge-Reihenfolge (Engine)

Szene / Submaster / Pult (HTP) → **Effect** (Move-Patterns, LTP) → Sonde-Override → Grand Master (Intensitätsfamilie) → Blackout.

---

## 4. Was im Repo bereits gebaut ist (v0.3)

### Technik

- .NET 8 + Avalonia 11, dunkles UI
- `DmxEngine` 40 Hz
- Outputs: Simulation, Air2DMX, Art-Net Broadcast/Unicast, Enttec Pro/USB2DMX
- Sonde, Fixture-JSON, Patch, Pult A/B, Szenen, Submaster
- **Move**: Fokuspunkte, A↔B-Patterns, Pfeiltasten (Shift = fein)
- Persistenz: `shows/*.json`, `fixtures/*.json`
- Skripte: `scripts/run.ps1`, `scripts/publish-standalone.ps1`

### Docs

- `README.md` — Einstieg
- `docs/LAPTOP.md` — Setup Laptop + erster Spot-Test
- `docs/outputs.md` — Air2DMX / USB
- `docs/architecture.md` — Module
- `docs/licon-move.md` — Licon vs. A↔B
- **Diese Datei** — Chat-Handoff

### Noch Platzhalter / nicht gebaut

- Assistent Live (Loopback) + Listen (Mood-Modelle)
- Spotify Now Playing / Titel-Bibliothek / Play mit|ohne
- Sonde-UI für **Bereiche** am Fader komfortabel (Datenmodell `ChannelRange` existiert, UI noch dünn)
- Windows-Installer
- Mehrere Universen, sACN, Open-DMX-Bitbang

---

## 5. Erster Hardware-Test (Laptop + U'King + Air2DMX)

1. Spot: DMX-Adresse **001**, Kanalmodus am Gerät  
2. Laptop → WLAN `JMS_AIR2DMX_…`  
3. Busk Desk → **Ausgang** → **Air2DMX** → Verbinden  
4. **Sonde**: Kanäle ausmessen, Rollen (Pan/Tilt/Dimmer/RGB/…) setzen, speichern  
5. **Patch** → **Pult** / **Move**  

Wenn Kanäle „tot“: oft Modus-/Sound-Kanal — auf niedrigen Wert (DMX manuell).

---

## 6. Audio / Spotify (nur Planung, nicht implementiert)

- **Live Pflicht:** WASAPI-Loopback  
- **Listen:** Song einmal durchhören → Show speichern  
- **Spotify:** Track-ID für „Song bekannt“; optional Connect Play mit/ohne (Premium)  
- Ein Rechner: Spotify Desktop + Busk Desk  
- ToS-Risiko Spotify bewusst akzeptiert; Architektur: Spotify = Identität, Licht = lokal  

---

## 7. Empfohlene nächste Schritte (Laptop)

Priorität grob:

1. Hardware-Test Spot + Air2DMX, Bugs fixen  
2. Sonde: Bereiche (0–120 = …) in der UI richtig bedienbar machen  
3. Move an echten Moving Heads testen (Pan/Tilt-Rollen Pflicht)  
4. UI polish (nach Kernfunktionen)  
5. Live-Audio-Assistent / Listen  
6. Spotify-Titel-Schicht  
7. Windows-Setup  

---

## 8. Für den Agenten auf dem Laptop

```text
Projekt: Busk Desk (https://github.com/judged34d/BuskDesk)
Lies: README.md, docs/CHAT-HANDOFF.md, docs/LAPTOP.md, docs/outputs.md, docs/licon-move.md
Arbeitsregel: schlankes Small-Show-Tool, keine Profi-Ballast-Features
Nächster sinnvoller Schritt: nach Nutzerwunsch — oft Hardware-Test oder Sonde-Bereiche-UI
```

Clone:

```powershell
git clone https://github.com/judged34d/BuskDesk.git
cd BuskDesk
.\scripts\run.ps1
```

---

## 9. Bewusst verworfene / zurückgestellte Ideen

- Webcam erkennt automatisch Farben/Gobos/Bewegung → zu unzuverlässig/aufwendig; manuelle Sonde bleibt  
- Vorgefertigte Fixture-JSONs im Repo → entfernt  
- Reine Portable-only-Strategie → Setup später bevorzugt  
- Licon-geometrische Patterns (Kreis/Lissajous) → nicht Priorität; A↔B zuerst  
