# Szenen-Spec — Smoketest & Anker-Test (für 02.11.)

Ziel: Zwei minimale Szenen, per MCP gebaut, die M0 (Pipeline) und M1 (Anker-Persistenz) abdecken. Kein UI, keine Features — nur die zwei Unbekannten des Projekts. Ablauf/Beweise: `docs/07-smoketest-protokoll.md`.

> API-Check 01.11.: Pinch-API offiziell bestätigt (Meta-Tutorial „unity-tutorial-basic-hand-training": `GetFingerIsPinching(OVRHand.HandFinger.Index)`); Spatial-Anchors-Tutorial existiert („unity-spatial-anchors-basic-tutorial"); SDK-Referenz **v207** ist online. Exakte Namen am 02.11. gegen die installierte SDK-Version prüfen — TODOs im Code.
> Ohne Gerät iterierbar: Die Mocks in `Runtime/Mocks/` + der `CaptureDriver` testen den kompletten Flow im Editor (EditMode-Tests), bevor der erste echte Adapter läuft.
> Ohne GerÃ¤t iterierbar: Die Mocks in `Runtime/Mocks/` + der `CaptureDriver` testen den kompletten Flow im Editor (EditMode-Tests), bevor der erste echte Adapter lÃ¤uft.

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
| `Adapters/OvrAnchorProvider.cs` | `IAnchorProvider` via OVRSpatialAnchor (Create/Save/Load) — Skeleton steht, TODOs gegen v207 |
| `Adapters/OvrHandSource.cs` | `IHandSource` via OVRHand — Pinch-API verifiziert, Bone-IDs am Gerät tunen |
| `Adapters/SceneWallProvider.cs` | `IWallProvider` via Scene API — Skeleton, erst nach M1 |
| `Runtime/Demo/AnchorTestController.cs` | Nutzt nur Interfaces (bleibt SDK-frei); fliegt nach M1 wieder raus |

**Regel:** Erst wenn beide Szenen ihre Pass-Kriterien erfüllen, wird am Capture-Flow gebaut.
