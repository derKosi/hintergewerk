# Szenen-Spec — Smoketest & Anker-Test (für 02.11.)

Ziel: Zwei minimale Szenen, per MCP gebaut, die M0 (Pipeline) und M1 (Anker-Persistenz) abdecken. Kein UI, keine Features — nur die zwei Unbekannten des Projekts. Ablauf/Beweise: `docs/07-smoketest-protokoll.md`.

## Szene 1 — `01_Smoketest.unity` (M0)

**Hierarchy:**
```
01_Smoketest
├── Meta XR Rig          (OVRCameraRig + OVRManager; Tracking-Origin: Floor)
│   └── Hände            (OVRHand L/R + OVRSkeleton, Tracking-Type: Hands)
├── Referenzwürfel       (0.15 m, Position 0 / 1.2 / 0.35, emissive grün)
└── StatusText           (TMP, World Space, 0 / 1.6 / 0.3, Text: „Smoketest — Hände sichtbar?")
```

**Pass:** APK baut · Channel „Competition" nimmt es · Invite-URL installiert · App startet · beide Hände rendern.
**Beweis:** Screenrecording vom Handy auf dem Headset.

## Szene 2 — `02_AnkerTest.unity` (M1)

**Hierarchy:** wie Szene 1, plus:
```
├── AnkerTest            (Script AnchorTestController)
│   ├── AnkerCube        (Prefab 0.06 m, rot, emissive)
│   └── OffsetCube       (Prefab 0.06 m, weiß, +0.5 m X relativ zum Anker)
```

**Verhalten `AnchorTestController` (Pseudocode):**
1. `Start()` → Adapter `LoadAll()` → Status: „Anker geladen: N"
2. Pinch (rechte Hand) → Anker an Zeigefingerpose erzeugen → `PersistAll()` → roten Cube an Ankerpose, weißen Offset-Cube als Kind → UUID ins Log
3. Jede Aktion aktualisiert den On-Screen-Text (N Anker, letzte UUID)

**Ablauf:** 3 Runs (je: setzen → beenden → neu starten → Lage prüfen ± 2 cm) — Run 2 mit Headset ab/auf, Run 3 mit Raumwechsel. Beweis je Run: Foto + UUIDs aus dem Log.

## Dateien, die morgen dazukommen

| Datei | Zweck |
|---|---|
| `Runtime/Adapters/OvrAnchorProvider.cs` | `IAnchorProvider` via OVRSpatialAnchor (Create/Save/Load) |
| `Runtime/Adapters/OvrHandSource.cs` | `IHandSource` via OVRHand (Pinch, Pointer-Pose) |
| `Runtime/Adapters/SceneWallProvider.cs` | `IWallProvider` via Scene API — erst nach M1 |
| `Runtime/Demo/AnchorTestController.cs` | Nur für die Testszene; fliegt nach M1 wieder raus |

**Regel:** Erst wenn beide Szenen ihre Pass-Kriterien erfüllen, wird am Capture-Flow gebaut.
