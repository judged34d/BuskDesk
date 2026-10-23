# Busk Desk auf dem Laptop (Windows)

**Arbeitsmaschine ist dein Windows-PC/Laptop**, Ordner `BuskDesk` (Clone dieses Repos).  
Cursor Cloud / Linux-VMs sind **nicht** die Entwicklungs-Maschine — dort keine Launcher, keine linux-x64-Builds als Produktpfad.

Dieses Repo ist die **einzige** Arbeitskopie. Nicht parallel an einem zweiten PC weiterentwickeln.

## 1. Einmalig einrichten

1. [Cursor](https://cursor.com) (Desktop) installieren  
2. [Git](https://git-scm.com/download/win) installieren  
3. [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0) installieren (zum Bauen)  
4. Repo clonen:

```powershell
cd $env:USERPROFILE\Projects   # oder ein anderer Ordner deiner Wahl
git clone https://github.com/judged34d/BuskDesk.git BuskDesk
cd BuskDesk
```

5. In Cursor: **File → Open Folder** → den Ordner `BuskDesk`

## 2. App bauen & starten

```powershell
.\scripts\run.ps1
```

Oder self-contained (eine Ordner-Exe, .NET Runtime eingebettet):

```powershell
.\scripts\publish-standalone.ps1
.\publish\win-x64\BuskDesk.exe
```

Desktop-Verknüpfung (Windows `.lnk`, nicht Linux `.desktop`):

```powershell
.\scripts\create-desktop-shortcut.ps1
```

Nutzt `publish\win-x64\BuskDesk.exe` falls vorhanden, sonst `scripts\run.ps1`.

## 3. Erster Test: U'King LED-Spot + Air2DMX

1. Spot auf **DMX-Adresse 001**, Kanalmodus am Gerät einstellen  
2. Laptop mit Air2DMX-WLAN `JMS_AIR2DMX_…` verbinden  
3. Busk Desk → **Ausgang** → **Air2DMX** → **Verbinden**  
4. **Sonde**: Startadresse 1, Kanalzahl = Modus am Spot, Fader bewegen und Reaktion notieren  
5. Rollen zuweisen → **Als Fixture speichern** → unter **Patch** einsetzen  
6. Wenn Kanäle „tot“ wirken: oft steckt ein Modus-/Sound-Kanal hoch — in der Sonde auf niedrigen Wert (DMX manuell)  

Keine mitgelieferten Hersteller-Profile; die Sonde ist der Weg.

## 4. Weiterarbeiten

Nur noch lokal unter Windows im Ordner `BuskDesk` committen und pushen. Keine parallelen Änderungen woanders; Cloud Agent nur für kurze Remote-Hilfe, nicht als Dauer-Arbeitsplatz.
