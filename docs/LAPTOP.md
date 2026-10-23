# Busk Desk auf dem Laptop

Dieses Repo ist die **einzige** Arbeitskopie ab jetzt. Am alten PC nicht weiterentwickeln.

## 1. Einmalig einrichten

1. [Cursor](https://cursor.com) installieren  
2. [Git](https://git-scm.com/download/win) installieren  
3. [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0) installieren (zum Bauen)  
4. Repo clonen (URL steht in README nach dem Push):

```powershell
cd $env:USERPROFILE\Projects
git clone https://github.com/judged34d/BuskDesk.git BuskDesk
cd BuskDesk
```

5. In Cursor: **File → Open Folder** → `BuskDesk`

## 2. App bauen & starten

```powershell
.\scripts\run.ps1
```

Oder self-contained (eine Ordner-Exe, .NET Runtime eingebettet):

```powershell
.\scripts\publish-standalone.ps1
.\publish\win-x64\BuskDesk.exe
```

## 3. Erster Test: U'King LED-Spot + Air2DMX

1. Spot auf **DMX-Adresse 001**, Kanalmodus **7CH** (oder 8CH — dann anderes Profil patchen)  
2. Laptop mit Air2DMX-WLAN `JMS_AIR2DMX_…` verbinden  
3. Busk Desk starten → **Ausgang** → **Air2DMX** → **Verbinden**  
4. **Sonde**: Startadresse 1, Dimmer + Rot hochziehen  
5. Wenn nichts passiert: Modus-Kanal (Ch7) auf **0–50** (DMX manuell), Sound-Modus aus  

Mitgelieferte Profile: `fixtures/uking-par-rgbw-{4,7,8}ch.json`  
Demo-Patch: Spot „U'King PAR 1“ ab Adresse 1 (7ch).

## 4. Weiterarbeiten

Nur noch auf dem Laptop committen und pushen. Am Server-PC keine parallelen Änderungen.
