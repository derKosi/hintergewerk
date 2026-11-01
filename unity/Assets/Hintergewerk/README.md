# Unity-Assets — Drop-in für das Hintergewerk-Projekt

Dieser Ordner wird **morgen (02.11.) nach dem Projekt-Setup** komplett nach
`<UnityProjekt>/Assets/Hintergewerk/` kopiert. Alles hier ist **SDK-frei**
(kein `OVR*`-Import) und kompiliert unabhängig vom Meta XR SDK.

## Dateien & Zweck

| Datei | Schicht | Inhalt |
|---|---|---|
| `Data/HouseModel.cs` | Kern | Datenmodell schema_version 1 — rein C#, anker-lokale Geometrie |
| `Data/SimpleJson.cs` | Kern | Handgeschriebener JSON-Writer/Parser (stabil, Invariant Culture, Forward-kompatibel) |
| `Data/HouseDataStore.cs` | Kern | CRUD + Auto-Save (atomar), korrupte Dateien werden gesichert, nie gelöscht |
| `Core/Interfaces.cs` | Naht | `IAnchorProvider`, `IHandSource`, `IWallProvider` — die 3 Adapter-Schnittstellen zum Meta SDK |
| `Capture/SnapEngine.cs` | Kern | Ebene→Raster→Magnet-Snap + Zwei-Hand-Tiefe |
| `Recall/CorridorFactory.cs` | Kern | Korridor-Ribbon als Mesh + anker-lokal→Weltposition |
| `Recall/WarningSystem.cs` | Kern | Hand↔Korridor-Distanz, Eskalationsstufen (None/Near/Critical) |
| `Export/PdfEmitter.cs` | Kern | PDF 1.4 in reinem C# (A4, Base14-Fonts, xref exakt) — Risiko R4 |
| `Export/MapRenderer.cs` | Kern | Grundriss-Projektion top-down: Wände, Korridor-Halo, Maßstab, Legende |
| `Editor/CoreSelfTest.cs` | Test | Menü „Hintergewerk": JSON-Roundtrip + PDF-Beispiel — **nach dem Import als Erstes laufen lassen** |

## Reihenfolge morgen

1. Unity-Projekt + Meta XR SDK aufsetzen (Guide: `docs/06-setup-guide.md`)
2. Diesen Ordner nach `Assets/` kopieren → kompilieren lassen
3. **Menü `Hintergewerk → Kern-Selbsttest (JSON-Roundtrip)`** → muss „bestanden" melden
4. **Menü `Hintergewerk → PDF-Beispiel erzeugen`** → PDF im Finder prüfen (öffnen!)
5. Erst DANN: SDK-Adapter implementieren (`OvrAnchorProvider`, `OvrHandSource`, `SceneWallProvider`) und
   die Minimal-Szene für den Smoketest bauen (Protokoll: `docs/07-smoketest-protokoll.md`)

## Konventionen (bitte einhalten)

- **Kein SDK-Import außerhalb der Adapter-Dateien.** Der Kern bleibt ohne `OVR*` testbar.
- Geometrie **immer anker-lokal** speichern (`Punkt_world = AnchorPose × Punkt_lokal`).
- `confidence` ist in der UI immer sichtbar; verwaiste Anker ⇒ „nicht verortbar", nie löschen.
- Auto-Save nach jeder Mutation — es gibt keinen Save-Button.

## Bekannte Grenzen (ehrlich)

- Code wurde **außerhalb von Unity geschrieben und noch nie kompiliert** — erste Kompilierung
  und ggf. Fixes passieren morgen nach dem Import (deshalb der Selbsttest als Schritt 3).
- `MapRenderer` zeichnet keine Beschriftungs-Kollisionsvermeidung (Labels können sich überlappen).
- `PdfEmitter` transliteriert Umlaute (ae/oe/ue/ss) — bewusst, um Encoding-Fehler auszuschließen.
