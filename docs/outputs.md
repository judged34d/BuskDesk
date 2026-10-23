# DMX-Ausgänge

Busk Desk nutzt eine Plugin-Schnittstelle `IDmxOutput`. Neue USB-Geräte = neue Implementierung, UI wählt nur die ID.

## Air2DMX (JMS)

- **Kein USB-DMX-Protokoll.** USB = 5 V Strom.
- Empfängt **Art-Net** per WLAN und gibt 1 Universum DMX aus.
- Hotspot-SSID: `JMS_AIR2DMX_…`, Setup oft unter `http://2.0.0.1`.
- In Busk Desk: Ausgang **Air2DMX** → Ziel `2.0.0.1:6454`, Universe 0.

## Art-Net Broadcast / Unicast

- Generische Nodes im LAN.
- Unicast: IP unter „Art-Net Host“ setzen.

## USB DMX Pro / USB2DMX

- Enttec DMX USB Pro API (`7E` … Label 6 … `E7`).
- Viele FTDI-Clones inkl. **JMS USB2DMX PRO** (in QLC+ oft als Pro RX/TX).
- COM-Port wählen, Treiber von JMS/Enttec installieren.

## Später

- Open DMX (Bitbang FTDI)
- sACN / E1.31
- Weitere Hersteller über dieselbe `IDmxOutput`-Schnittstelle
