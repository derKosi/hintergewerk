# Architektur — Hintergewerk

## 1. Stack (D4, GO 01.11.)

| Ebene | Wahl | Warum |
|---|---|---|
| Engine | **Unity 6 LTS** | Beste Hand-Tracking-Qualität, Spatial Anchors, Store-Pfad |
| SDK | Meta XR All-in-One (v207+) | Hände, Interaktion, Anker, Passthrough aus einer Hand |
| Build | Android APK, OpenXR, Quest 3 | Wettbewerbsvorgabe; läuft auf Horizon-OS-Geräten |
| Iteration | XR Simulator + Gerät | Simulator für Tempo, Gerät für Wahrheit (ab M1 täglich 1 Lauf) |
| Persistenz | Meta Anchor Storage (lokal) + JSON | Kein Backend, kein Cloud-Zwang (Scope!) |
| Repo | GitHub/Forgejo (D7) | Code + Planung; Dashboard ist nur Build-Hub/CDN |

Kein WebXR-Umweg: Die riskanteste Annahme ist **Anker-Persistenz**, nicht Zeichnen — und die ist nativ am stärksten.

## 2. Module

```
Assets/Hintergewerk/
├── Runtime/                 SDK-freier Kern (asmdef Hintergewerk.Runtime)
│   ├── Core/                Interfaces (IAnchorProvider, IHandSource, IWallProvider), AppState
│   ├── Capture/             SnapEngine, CaptureSession, CaptureDriver, UndoStack
│   ├── Data/                HouseModel, SimpleJson, HouseDataStore
│   ├── Recall/              CorridorFactory, WarningSystem
│   ├── Localization/        Strings (DE/EN)
│   ├── Mocks/               InMemory-Implementierungen der Interfaces (Editor-Iteration ohne Gerät)
│   ├── Demo/                AnchorTestController (M1-Demo, fliegt nach M1 raus)
│   └── Export/              PdfEmitter, MapRenderer
├── Editor/                  Selbsttest-Menü + Batch-Build (asmdef Hintergewerk.Editor)
├── Tests/EditMode/          NUnit-Tests (asmdef Hintergewerk.Tests) + Fixtures
└── Adapters/                OVR*-Implementierungen der Interfaces (Assembly-CSharp — bewusst außerhalb der Runtime-asmdef)
```

| Klasse | Verantwortung |
|---|---|
| `AnchorManager` | Anker erzeugen/speichern/laden; Registry `anchor_id → Pose`; liefert `anchor_confidence` |
| `DrawController` | Pinch → Polylinie (Punkte in **anker-lokalen** Koordinaten) |
| `SnapEngine` | Raycast auf Wand-/Deckenebene (Scene Mesh, sonst kalibrierte Ebene); 5-cm-Raster; magnetische Endpunkte |
| `DepthGesturer` | Zwei-Hand-Distanz → `depth_cm`; Typ-Presets als Startwert |
| `ObjectRepository` | CRUD auf Objekten; JSON in `persistentDataPath`; `schema_version` |
| `CorridorFactory` | Korridor (± 5 cm) als leuchtendes Volumen statt Linie; Typfarben |
| `WarningSystem` | Annäherung Hand ↔ Korridor: visuelle Eskalation + optional räumlicher Audio-Ping (FoV-aware) |
| `EvidenceCapture` | Foto-Anhang (Ansatz D8), Sprachnotiz (optional) |
| `PdfEmitter` | PDF in reinem C#: Polylinien in Grundriss-Projektion, Legende, Base14-Fonts — keine externe Dependency |

**Koordinaten-Konvention (tragende Entscheidung):** Geometrie wird **anker-lokal** gespeichert, nie world-space. Anker-Pose-Änderungen (Re-Lokalisierung, Drift) überleben so; beim Laden gilt `Punkt_world = AnchorPose × Punkt_lokal`.

## 3. Datenmodell (JSON, `schema_version: 1`)

```json
{
  "schema_version": 1,
  "rooms": [{ "id": "raum_eg_bad", "label": "Bad EG", "floor": 0 }],
  "anchors": [{ "id": "anc_...", "room_id": "raum_eg_bad", "confidence": "high" }],
  "objects": [{
    "id": "obj_...",
    "type": "power | water | heating | stud | no_drill_zone",
    "room_id": "raum_eg_bad",
    "anchor_id": "anc_...",
    "geometry": { "points_lokal": [[0.0, 1.2, 0.0], [0.5, 1.2, 0.0]], "depth_cm": 4.0, "diameter_cm": 2.5 },
    "confidence": "measured | estimated | scanner",
    "safety_corridor_cm": 5.0,
    "evidence": { "photos": ["photos/obj_..._1.jpg"], "voice_note": null },
    "meta": { "created_at": "2026-11-05T14:32:00Z", "author": "Mathias", "note": "" }
  }]
}
```

Regeln: `confidence` ist in der UI **immer sichtbar**. Fotos liegen als Dateien unter `persistentDataPath/photos`, referenziert relativ. Auto-Save nach jeder Änderung — kein Save-Button.

## 4. Persistenz-Flow

1. App-Start → `AnchorManager` lädt gespeicherte Anker (lokal) → Posen bereit
2. `ObjectRepository` lädt JSON → Objekte binden an `anchor_id` (lokal → world)
3. **Verwaiste Objekte** (Anker fehlt): nicht raten — in der Liste als „nicht verortbar" führen, Konfidenz sichtbar herabstufen (Ehrlichkeitsregel)
4. Speichern: inkrementell, automatisch

Anker-Regel: Anker nur an stabilen Merkmalen setzen (Türzarge, Zählerschrank, Bodenplatte), niemals an Möbeln.

## 5. Export

- **JSON**: 1:1 das HouseModel — maschinenlesbar, überlebt Jahre via `schema_version`
- **PDF**: Grundriss-Projektion (Top-Down x/z) aller Objekte eines Raums, Maßstab + Legende (Typ, Farbe, Tiefe, Konfidenz), Monteur + Datum. Eigener `PdfEmitter` (Linien + Helvetica). Fallback: PNG-Render derselben Ansicht.
- **BIM-Anschluss (D40, ADR-0005):** Das JSON ist der **Anschlussvertrag** für Planungs-/BIM-Workflows — strukturiert, versioniert, mit den Feldern, die Planung braucht (Typ · Tiefe · Konfidenz · Monteur · Zeit · anker-lokale Geometrie). **Kein IFC-Export im Wettbewerbsscope** — Pitch-Sprache „BIM-ready", nie „BIM". Der PDF-Grundriss ist die menschenlesbare Ebene derselben Daten; dieselbe Projektion wird mit D39 live als Bauplan-Panel in der Hand gerendert.

## 6. Build- & Delivery-Pipeline

```
Unity (APK) → Meta Developer Dashboard → Release-Channel „Competition" → Invite-URL → Quest (Install über URL) → Devpost-Submission
```

- Signing: Dev-Key; Upload manuell (bewusst ohne API-Automatisierung — ein APK, ein Upload)
- Invite-URL vor der Abgabe einmal **komplett frisch** testen (M5)

## 7. Raumerfassung: Scene API statt Scan-to-BIM (D15)

Wir bauen **keinen** Scan→BIM→Blueprint-Pfad — die Logik ist umgedreht: **Der Blueprint ist Output, nicht Input.**

- **Erfassung:** Quest Room Setup / Scene API liefert Wände, Decke, Boden als klassifizierte Ebenen + Scene Mesh. Einmal pro Raum, OS-seitig. Das ist unser kompletter „Scan“.
- **Verortung:** Objekte werden live an die realen Ebenen gesnappt (SnapEngine) und an Ankern fixiert. Kein vorausgesetztes Bestandsmodell nötig.
- **Raum-Zuordnung automatisch (D43):** Die gesnappte Wand trägt `RoomId`/`RoomLabel` aus der Scene API (`OVRSceneRoom`-Elternkette, Raum-Id = Room-Anchor-UUID, Label aus der Klassifikation); der erste gezeichnete Punkt sortiert das Objekt in diesen Raum. Ein im Room Setup neu angelegter Raum wird so ohne Bedien-Schritt erfasst. Wände ohne erkannten Raum fallen auf den Standard-Raum (nichts erfunden); Stockwerke: `floor` im Datenmodell vorhanden, Erfassung ist Roadmap (§7.1).
- **Grundriss als Export:** Erst beim Export erzeugt der `MapRenderer` den Plan — Schnitt durch das Scene Mesh auf ~1 m Höhe → Wandpolygon, Objekte top-down projiziert, Maßstab + Legende → PDF/PNG. Der Plan wächst mit dem Gedächtnis und zeigt nur, was erfasst wurde (Ehrlichkeitsregel).
- **Grenzen der Hardware:** Depth API = Laufzeit-Tiefe, kein persistenter Raumscan; Passthrough von Apps nicht aufzeichnungsfähig (vgl. R3); keine LiDAR-Hardware auf Quest 3.

**Roadmap (nicht MVP):** Import von LiDAR-Scans (Apple RoomPlan, Polycam, Scaniverse) für höhere Wandgenauigkeit · IFC-/BIM-Export für Übergabe an Planungsbüros · Scan-to-BIM (Matterport, BLK360, ReCap/Revit) bleibt Profi-Werkzeug. Fürs Video genügt ein Satz unter „What's next“.

### 7.1 Onboarding & Geist-Modus (D16)

Der „Raum scannen“-Moment des Nutzers ist das **OS-seitige Room Setup**, nicht ein eigenes Scan-Modul (kein LiDAR auf Quest 3, Fotogrammetrie ohne Semantik wäre fürs Snappen unterlegen):

- **Onboarding:** Beim Start prüft die App, ob Scene-Daten für den Raum existieren. Fehlen sie → System-Room-Setup-Flow (Raum abgehen, Wände nachfahren). Danach stehen klassifizierte Ebenen + Möbel-Semantik (OVRScene-Anker) bereit.
- **Geist-Modus (Ghost View):** Umschaltbare Ansicht des erkannten Raums — Wände als Wireframe, Möbel als semantische Volumen. Darin lassen sich Möbel-Geister per Hand-Geste wegschieben, um die Wand für Raycasts/Anker „freizuräumen“; dann zurück in den Passthrough und an der echten Wand zeichnen. Video-Beat: 5 s, zeigt Scene-API-Beherrschung.
- **Ehrliche Grenzen:** Reale Möbel bleiben im Passthrough sichtbar (Kamerabild nicht editierbar, vgl. R3); vom Möbel verdeckte Flächen wurden nie gescannt (Oklusionslöcher); Room Setup ist pro Raum — Treppen/Mehrstock sind Roadmap.
- **Priorität:** Onboarding = Fix · Geist-Modus + Möbel-Push = **Stretch nach M2** (D16-Gate: nur wenn Capture-Flow stabil).

## 8. Explizit Out of Scope

Automatische Erkennung · Scan-to-BIM / Raum-Scanning (LiDAR, Photogrammetrie, IFC-Generierung) · BIM/IFC-Import · Multi-User/Cloud · Accounts · Store-Release · VR-Glasses-Spezifika · jede Form von „Durch-Wände-Sehen“.

## 9. Test-Strategie

- XR Simulator: tägliche Iteration (Zeichnen, UI, Export)
- Gerät: ab 02.11. täglich ein Lauf; Checklisten in `03-interaktion.md`
- Jeder Meilenstein M0–M5 hat einen **Beweis** (Recording/Datei), nicht nur ein Gefühl
