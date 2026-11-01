# Unity-Assets — Drop-in für das Hintergewerk-Projekt

Dieser Ordner wird beim Projekt-Setup komplett nach `<UnityProjekt>/Assets/Hintergewerk/` kopiert.
Der Runtime-Kern ist **SDK-frei** (kein `OVR*`-Import) und kompiliert unabhängig vom Meta XR SDK;
die Assemblies sind über asmdefs getrennt (schnelle Kompilierung, klare Grenzen).

## Struktur

```
Hintergewerk/
├── Runtime/                 asmdef Hintergewerk.Runtime  — SDK-freier Kern
│   ├── Core/                Interfaces.cs: IAnchorProvider, IHandSource, IWallProvider
│   ├── Data/                HouseModel, SimpleJson, HouseDataStore
│   ├── Capture/             SnapEngine
│   ├── Recall/              CorridorFactory, WarningSystem
│   └── Export/              PdfEmitter, MapRenderer
├── Editor/                  asmdef Hintergewerk.Editor — Selbsttest-Menü + Batch-Build
└── Tests/EditMode/          asmdef Hintergewerk.Tests — NUnit-Tests
```

## Reihenfolge beim Setup

1. Unity-Projekt + Meta XR SDK aufsetzen (Guide: `docs/06-setup-guide.md`, Bundle-ID `de.bautoolz.hintergewerk`)
2. Diesen Ordner nach `Assets/` kopieren → kompilieren lassen
3. Menü **Hintergewerk → Kern-Selbsttest (JSON-Roundtrip)** → muss „bestanden" melden
4. Window → General → **Test Runner** → EditMode → alle Tests grün (JSON, Snap, Warnung, PDF)
5. Menü **Hintergewerk → PDF-Beispiel erzeugen** → PDF im Finder prüfen
6. Erst DANN: SDK-Adapter (`Runtime/Adapters/`) und die Smoketest-Szenen aus `docs/11-szene-spec-smoketest.md`

## Dateien

| Datei | Schicht | Inhalt |
|---|---|---|
| `Runtime/Data/HouseModel.cs` | Kern | Datenmodell Schema v1 — reines C#, anker-lokale Geometrie |
| `Runtime/Data/SimpleJson.cs` | Kern | Handgeschriebener JSON-Writer/Parser (Invariant Culture, Forward-kompatibel) |
| `Runtime/Data/HouseDataStore.cs` | Kern | CRUD + Auto-Save (atomar), korrupte Dateien werden gesichert |
| `Runtime/Core/Interfaces.cs` | Naht | `IAnchorProvider`, `IHandSource`, `IWallProvider` — die SDK-Adapter-Schnittstellen |
| `Runtime/Capture/SnapEngine.cs` | Kern | Ebene→Raster→Magnet-Snap + Zwei-Hand-Tiefe |
| `Runtime/Recall/CorridorFactory.cs` | Kern | Korridor-Ribbon als Mesh + anker-lokal→Weltposition |
| `Runtime/Recall/WarningSystem.cs` | Kern | Hand↔Korridor-Distanz, Eskalationsstufen |
| `Runtime/Export/PdfEmitter.cs` | Kern | PDF 1.4 in reinem C# (A4, Base14, exakte xref) |
| `Runtime/Export/MapRenderer.cs` | Kern | Grundriss-Projektion top-down mit Legende und Maßstab |
| `Editor/CoreSelfTest.cs` | Test | Menü „Hintergewerk": JSON-Roundtrip + PDF-Beispiel |
| `Editor/BuildScript.cs` | Build | Batch-Build für SSH/Remote (`-executeMethod`) |
| `Tests/EditMode/*.cs` | Test | 15 Tests über JSON, Snap, Warnung, Export |

## Konventionen (AGENTS.md gilt)

- **Kein SDK-Import außerhalb der Adapter-Dateien** (nach dem Umbau: `Runtime/Adapters/`).
- Geometrie **immer anker-lokal** speichern.
- `confidence` immer sichtbar; verwaiste Anker ⇒ „nicht verortbar", nie löschen.
- Auto-Save nach jeder Mutation.

## Bekannte Grenzen (ehrlich)

- Code wurde **außerhalb von Unity geschrieben und noch nie kompiliert** — erste Kompilierung
  und Fixes passieren beim Import (deshalb Selbsttest + Test Runner als Schritte 3/4).
- `MapRenderer` hat keine Beschriftungs-Kollisionsvermeidung (Labels können sich überlappen).
- `PdfEmitter` transliteriert Umlaute (ae/oe/ue/ss) — bewusst, gegen Encoding-Fehler.
