# HANDOFF für Claude (Windows · Unity · RTX 3090)

## Update 08.11. (Replan: „Bauplan in der Hand" + BIM-ready — D38–D40, ADR-0005)

**Nachmittag (autonom): M4 CODE-SEITIG KOMPLETT vor Feature-Freeze:**
1. **Szene 03 + Bauplan live beim Zeichnen:** `HouseDataStore.Reload()` im Panel-Poll —
   gezeichnete Objekte erscheinen OHNE Neustart im Plan (der D39-Kern!). Szene 03 verdrahtet
   (OvrAnchorProvider hängt dort am DrawController-GO, nicht an HGW_Adapters).
   Beweis: `s03_plan_live.png` — gelbe Elektro-Linie + „Geladen: 1 Objekte" direkt nach DrawSquare.
2. **FoV-Audio:** `WarningAudio.PanFor` — Ping pannet zur Gefahr (6 Tests); Simulator-Beweis
   `panStereo=-0.50` während Eskalation. RecallController reicht Korridor-Richtung durch.
3. **Export alle Räume:** `ExportController` mit `roomId=''` → ein Pinch = PDF je Raum + globales
   JSON (Logs: raum_1.pdf + raum_2.pdf + JSON 5 Objekte).
4. **docs/09 v2** (Bauplan + BIM-ready-Satz) · **docs/07 §G** Bauplan-Gerätetest-Checkliste ·
   **docs/08 Szene 2b** Video-Beat „plan grows in your hand" · **86/86 Tests** · Selbsttest ✓.
   **Neue APK nötig** (alter Stand 15:36 ohne Nachmittag-Features) → nach diesem Abschnitt bauen.

**M4 „zweiter Raum" BESTANDEN (Simulator):** RecallController + BauplanPanel akzeptieren
`roomId=""` = alle Räume; Seeder legt Raum 2 (Flur, eigene Wand + Anker + 2 Objekte) an.
Beweis: Log `4 Korridore, 1 nicht verortbar (Raum alle)` + Foto `m13_zweiRaeume.png`
(rot = Kritisch in Raum 1, grün = Ständerwerk in Raum 2, Bauplan-Panel im Bild).
Szene 04 läuft auf „alle Räume". **Offen:** Szene 03 (CaptureAutoTest) hat kein
`HGW_Adapters` — Bauplan-Panel dort braucht eigene Adapter-Verdrahtung (~30 min,
nicht kritisch: Szene 04 zeigt das Panel bereits; Video kann Szene 04 nutzen).

**Simulator-Testlauf M3 bestanden (Claude, mit Fotos + Fixes) — drei ECHTE Bugs gefunden und behoben:**
1. **Korridore unsichtbar (Backface-Culling):** BuildRibbon lieferte nur eine Dreiecks-Wicklung;
   URP/Unlit culled Backfaces → Korridore von der Kameraseite weg. Beide Wicklungen jetzt
   (Tests decken es ab). Beweis: `TestShots/m7_korridore.png` (blau) → `m7_eskalation.png` (rot).
2. **Z-Fighting + 0-Ausdehnungs-Bounds:** Ribbon koplanar zur Wand → unsichtbar; zusätzlich
   sortierte das XR-Frustum-Culling 0-Extent-Bounds aus. Fix: 8 mm kamera-seitiger
   Anzeige-Offset (`CorridorFactory.FacingOffset`) + Bounds aufblähen. ADR-0002 unberührt.
3. **Späte Anker-Lokalisierung:** Am Gerät kommen Posen erst NACH `LoadAll` an — Korridore
   blieben sonst dauerhaft „nicht verortbar". RecallController pollt jetzt (0,5 s) und rebuildet
   (`ShouldRebuildForLatePoses`, unit-getestet). Ehrlichkeitsregel bleibt: nur Rebuild wenn
   wirklich Posen dazukamen, Verwaiste nie zuholen.

**Harness-Regel (gilt für beide Agenten):** `RecallAutotest.Prepare()` VOR `editor.playmode start`
(im Edit-Modus) — im Play-Modus aufgerufen raced es gegen den Controller-Start (Beweis: Lauf
14:52 mit `0 Korridore, 3 nicht verortbar`). Fotos: `SimulatorShot.CaptureAt(...)` (manueller
Kamera-Render; XR rendert nicht in den Game-View — D33 bestätigt). Realer XR-Blick für Fotos:
TrackingSpace vor die Wand setzen + `capture game_view --source screen`.

**Bauplan-Panel (D39) gebaut:** `PlanProjection` (SDK-frei, testbar) + `BauplanPanel` (live
Top-Down an der linken Hand, Statuszeile aus Strings-Tabelle, estimated = 65 % Alpha) +
`BauplanWiring` (Menü Hintergewerk → Bauplan verdrahten). Szene 04 verdrahtet + gespeichert.

**Vision geschärft (Mathias):** Nicht nur Raum erfassen — der Monteur hält einen **Bauplan in der
Hand** (Live-Top-Down des Raums) und trägt Leitungen mit der freien Hand **hinter der Wand** ein;
das Ergebnis geht als **BIM-ready-Export** in die Bauplanung. Ehrlich gerahmt (D40): kein IFC bis
18.11. — das schema-versionierte JSON ist der Anschlussvertrag, Pitch-Sprache „BIM-ready", nie „BIM".

- **D38 Mitte-Scope-Check vorgezogen:** Scope-Leiter-Sprossen 1–3 gestrichen (Agenten-Vorschlag,
  4. Objektklasse, Sprachnotiz) — nichts davon war gebaut, kostet null.
- **D39 M4 v2:** `MapRenderer`-Projektion (existiert fürs PDF, top-down) **live** als Textur auf einem
  Hand-Panel — Wiederverwendung, kein neuer Pfad. Panel-als-Zeichenebene = Scope-Sprosse 4 (Stretch).
  **Neuer Plan:** docs/01 §3.2 (Replanung 08.11.) + Meilensteine v2. Feature-Freeze **13.11.**
- **Nächster Baustein (Agent):** Bauplan-Panel bauen — `MapRenderer` → `Texture2D`/RenderTexture auf
  Quad an der linken Hand, Update beim Confirm. Danach: M4-Rest (zweiter Raum, FoV-Audio).
- **Engpass bleibt Mathias' Headset-Block:** M1 3× Runs (🔴 seit 02.11. überfällig, GO/NO-GO D6) +
  M2 + M3 Tests (docs/07 §E/F) — einem Rutsch, ~2 h.
- **Nicht verwechseln:** `D:\Unity\Projects\YetAnotherDemoVR` ist ein SEPARATES Projekt (kein
  Hintergewerk-Bezug) — nur dessen Recording-/Video-Workflow wird fürs Abgabe-Video wiederverwendet.
- Invite-URL-Flow (Jury-Pfad) vor M5 nochmal testen — M0 lief bislang via adb install.



## Update 07.11. abends (Session 3, Claude auf der 3090 — M2/M3-Kern STEHT ✅)

**M3-Loop (Recall + Export) ist gebaut und im XR-Simulator verifiziert; Capture-Flow (M2) komplett
mit Zeichnen-Vorschau. ADR-0002-Regression behoben. Gerätetests M2/M3 warten auf Mathias (docs/07 §E/F).**

Was diese Session gebracht hat (alles mit Tests, 58/58 EditMode grün, Selbsttest grün):

1. **ADR-0002-Fix (kritisch):** CaptureDriver speicherte Welt-Punkte; Recall transformierte
   nochmal mit Anker-Pose → Korridore doppelt versetzt nach Neustart. Jetzt `WorldToLocal`
   beim Capture, Roundtrip-Tests (Translation + Rotation).
2. **Szene 04_Recall** (M3): Korridore in **Typfarben**, Warn-Eskalation (Bernstein→Signalrot,
   Puls, prozeduraler Audio-Ping), Export per linker-Hand-Pinch (PDF + JSON 1:1 HouseModel).
   Simulator-Beweis: 2 Korridore + 1 „nicht verortbar", Near→Kritisch geloggt, Dateien im
   persistentDataPath. `RecallDemoSeeder` (StandardExecutionOrder −100, **aus**): nur für
   Simulator-Tests, am Gerät laufen echte Daten.
3. **Zeichnen-Vorschau** (docs/03 „Linie folgt der Hand"): LineRenderer in Typfarbe + Snap-Marker;
   Auto-Start nur EINMAL (vorher: nach jedem Confirm sofort neuer Capture-Loop).
4. **SceneSwitcher:** linke Handfläche + linker Pinch = nächste Szene (kein adb) — Build-Liste
   02 → 03 → 04 → 01 (docs/05 D34).
5. **Beweisfoto-Seam** (D8/D33): `IEvidenceCapture` + `AppSnapshotEvidence`; ehrlicher Fallback
   „Foto nicht verfügbar" bewiesen. Ob ScreenCapture am Gerät (Vulkan) schreibt → Gerätetest.
6. Werkzeuge: `Scene04Builder` + `RecallAutotest` (Prepare/Cleanup) im Editor-Menü/eval;
   Build via Pipeline `unity command build run confirm=true` (in-Editor, kein Editor-Neustart).

Nächste Schritte: (a) Mathias: Gerätetests M2/M3 nach docs/07 §E/F + M1-Runs (3× Persistenz),
(b) Politur 11.–13.11.: FoV-Audio, zweiter Raum, Detail-Panel (Blick+Pinch), (c) Video-Material
aus Szene 03/04 als Beleg fürs Storyboard (docs/08).

---

# (Original-Handoff, Stand 04.11.)

Hallo Claude — du übernimmst ein laufendes 18-Tage-Hackathon-Projekt an Tag 11 von 18.
Der vorherige Agent (AutoClaw, Mac) hat Planung, Architektur und den kompletten SDK-freien
Code-Kern gebaut. Deine Mission: Unity-Seite zum Laufen bringen, Smoketest bestehen,
Capture-Flow finalisieren, dann Recall + Export. Der Mensch (Mathias) macht Gerät,
Dashboard, Video und Submission — seine Zeit ist knapp, rechne mit Zeitboxen.

## Das Projekt in 5 Sätzen

**Hintergewerk** dokumentiert verdeckte Installationen (Strom, Wasser, Heizung, Ständerwerk)
in VR, solange die Wand offen ist — gezeichnet mit den Händen, räumlich verankert, mit Foto,
Tiefe, Konfidenz und Monteur. Wochen später blendet die App das Gedächtnis der Wand als
Sicherheitskorridor ein. Export als PDF-Grundriss + JSON. Wettbewerb: Meta VR Start Developer
Competition 2026 (Devpost) — **Deadline 18.11.2026, 21:00 CET**. Track Productivity,
Division New Experience. Preisgeld-Gesamtpool $1M, Top-Preise $100k.

## LIES ZUERST (in dieser Reihenfolge, alle im Repo)

1. **AGENTS.md** — die 6 harten Invarianten. Nicht verhandelbar.
2. `docs/01-projektplanung.md` — Tagesplan, Meilensteine M0–M5, Risiko-Register, Scope-Leiter
3. `docs/02-architektur.md` — Stack, Module, Datenmodell, Persistenz (anker-lokal!)
4. `docs/05-entscheidungslog.md` — D1–D23: warum Dinge so sind, wie sie sind
5. `docs/03-interaktion.md` — die 5-Gesten-Grammatik + Testkriterien
6. `docs/11-szene-spec-smoketest.md` — exakte Szenen-Spec für M0/M1
7. `docs/07-smoketest-protokoll.md` — M0/M1 mit Pass-Kriterien und Beweisen
8. `docs/06-setup-guide.md` — Setup-Schritte (für Mac geschrieben, 1:1 auf Windows übertragbar)
9. `docs/12-devpost-felder.md` + `docs/08-video-voiceover.md` — Abgabe-Material (Menschenarbeit)

## Harte Invarianten (aus AGENTS.md, gekürzt)

1. **Ehrlichkeitsregel:** Kein „Wir sehen durch Wände". Jedes Objekt trägt sichtbare
   Konfidenz. Verwaiste Objekte (Anker weg) bleiben sichtbar „nicht verortbar", nie löschen.
2. **Anker-lokale Geometrie:** `Punkt_world = AnchorPose × Punkt_lokal`. Nie world-space speichern.
3. **Kein OVR\* in `Runtime/`:** SDK nur in `Assets/Hintergewerk/Adapters/` (Assembly-CSharp).
   Die Interfaces: `Runtime/Core/Interfaces.cs` (IAnchorProvider, IHandSource, IWallProvider).
4. **schema_version 1:** Parser überspringt unbekannte Keys. Änderung → Version hoch + Migration.
5. **Auto-Save** nach jeder Mutation. Kein Save-Button.
6. **Keine Secrets im Repo.** Keine erfundenen Messwerte, keine echten Gebäudedaten in der Demo.

## Aktueller Stand (04.11.2026)

**Existiert und getestet (51 EditMode-Tests, noch NIE kompiliert — Unity fehlte bislang):**
- `Runtime/Data/` — HouseModel (Schema v1), SimpleJson (handgeschrieben, invariant culture),
  HouseDataStore (Auto-Save, atomar, korrupte Dateien werden gesichert)
- `Runtime/Capture/` — SnapEngine (Ebene→5cm-Raster→Magnet), CaptureSession
  (5-Phasen-State-Machine), CaptureDriver (Frames rein → Store raus), UndoStack
- `Runtime/Recall/` — CorridorFactory (Ribbon-Mesh), WarningSystem (Eskalation), RecallController
- `Runtime/Export/` — PdfEmitter (PDF 1.4 in reinem C#, exakte xref), MapRenderer
  (Normblätter 1:20–1:200, Schriftfeld), ExportRunner
- `Runtime/UI/HandMenuLogic.cs` — Menü-Logik (Toggle, Cycle-Wrap, Close)
- `Runtime/Demo/AnchorTestController.cs` — M1-Demo (nutzt nur Interfaces)
- `Runtime/Localization/Strings.cs` — alle UI-Texte DE/EN (nichts hardcoded)
- `Adapters/` — OvrAnchorProvider, OvrHandSource, SceneWallProvider: **Skeletons mit
  TODO-Markierungen** — die API-Namen sind gegen SDK **v207** zu verifizieren!
- `Editor/` — CoreSelfTest (Menü: JSON-Roundtrip + PDF-Beispiel) + BuildScript
  (Batch-Build: `-executeMethod Hintergewerk.EditorTools.BuildScript.BuildAndroid`)
- `Tests/EditMode/` — 51 Tests + Fixtures (`Fixtures/*.json`)
- `website/` — 4-sprachige Promo-Site · `build.cmd` — Remote-Build-Runner

**Existiert NICHT:**
- Unity-Projekthülle (ProjectSettings/, Packages/manifest.json) — entsteht beim ersten Setup
- Kompilierte/bestätigte Anything — der Code lief NIE durch einen Compiler
- Szenen (01_Smoketest, 02_AnkerTest) — Specs stehen in docs/11
- Device-Verifikation — Smoketest M0/M1 nie gefahren
- Adapter-API-Namen — unbestätigt gegen v207

## Update 07.11. (Session 2, Claude auf der 3090 — M0 BESTANDEN ✅)

**M0-Smoketest auf Quest 3S bestanden: Hände getrackt, animiert, korrekt ausgerichtet; Text/Würfel sichtbar; Greif-Demo läuft.**

Root-Causes der Hand-Probleme (alle im Commit, gilt für beide Agenten):
1. `OVRRuntimeSettings.HandSkeletonVersion` stand auf `OpenXR` → Skeletons liefen als XRHand-Typen(4/5) gegen das OVR-Konventions-Mesh. FIX: version=OVR + `_skeletonType` 0/1.
2. **Metas OVRCustomHandPrefab liefert UNGBUNDEN aus** (`_customBones_V2` = Nulls) → ohne "Auto Map Bones" statisches Bind-Pose-Mesh. FIX: AutoMapBones per Editor (24/24 Knochen pro Hand, Overrides in 01_Smoketest.unity).
3. StatusText rotY 180°→0 (TextMesh lesbar von −Z).

Neue MBs in `Adapters/` (temp, vor Submission eval): HandDebugProbe (logcat), SmoketestPlacer (kopf-relativ: Tisch+Würfel+Stab+Text), ThrowCube+CollisionBlip (Pinch-Greif-Demo mit Audio), HandVisibilityDriver (Hand-Meshes aus bei Tracking-Verlust — "Geisterhand"-Fix).

Build-Reihenfolge getauscht: **App startet mit 02_AnkerTest** (M1-Runs), 01_Smoketest zweitens. Device-Loop: adb (Quest 3S, Ser. 340YC10G7T044V) — bei `unauthorized` Headset-Dialog bestätigen. Nächster Schritt: **M1-Anker-Persistenz, 3 Runs** (docs/07) — GO/NO-GO.

## Update 06.11. (Nacht-Session, Claude auf der 3090)

**Stand: Gerät-Pipeline komplett vorgebaut — Mathias muss nur noch Dashboard + Quest klicken (docs/07).**

- **Meta XR Core SDK 207.0.1** via Metas NPM-Registry (`npm.developer.oculus.com`, scoped
  `com.meta`) — kein Asset-Store-Klick nötig (D26)
- **Adapter gegen echte v207-API umgeschrieben und kompiliert** (D25 aufgelöst):
  AddComponent → auto-create · `SaveAnchorAsync()` · `LoadUnboundAnchorsAsync` + `BindTo` ·
  `anchor.Uuid` · `OVRSkeleton.BoneId` (Hand_WristRoot/Hand_IndexTip) · Quellen-Referenz: PackageCache
- **Szenen 01_Smoketest + 02_AnkerTest gebaut** (D28): SDK-Prefabs `OVRCameraRig` +
  `OVRCustomHandPrefab_L/R` unter den Hand-Anchors · Szene 02 mit verdrahtetem
  AnchorTestController (OvrAnchorProvider + OvrHandSource) · StatusText als TextMesh (kein TMP-Import)
- **Hand-Tracking an:** OculusProjectConfig `handTrackingSupport=ControllersAndHands`,
  `handTrackingFrequency=High` — OVR injected die HAND_TRACKING-Permission beim Build
- **MetaQuestFeature (OpenXR, Android) aktiv** + Meta XR Feature Set enabled
- **2× APK erfolgreich gebaut** (`build.cmd` → `unity/Builds/hintergewerk.apk`, IL2CPP/ARM64) —
  Build #2 enthält SDK + Szenen; Build #3 (läuft) zusätzlich mit Hand-Tracking-Config
- **51/51 EditMode-Tests** weiter grün (mit aktiven Adaptern — kein Regression)
- Editor-Fernsteuerung steht: Unity-CLI + Pipeline (script eval, Menüs, Szenen-Kommandos)

**Für Mathias (Morgen, ~5 min, kein Blocker):**
1. Meta XR Simulator installieren: Doppelklick auf `_hgw_unity_tmp/sim/meta_xr_simulator.msi`
   (435 MB, offizielles Meta-Paket v207.1) — danach Unity: `Window/Meta/Meta XR Simulator/Activate`
   (D29; Unitys Auto-Download scheitert hier an einem Datei-Lock in Downloads)
2. Dann docs/07: Dashboard-App + Channel „Competition" + APK-Upload + Invite-URL + Quest (Block C/D)

## Update 05.11. (Windows-Session, Claude auf der 3090)

- **Monorepo-Entscheid (D24):** Unity-Projekt lebt im Repo unter `unity/` — Projekthülle
  (ProjectSettings + Packages) eingecheckt, erstellt per Unity-CLI aus URP-Blank
- **51/51 EditMode-Tests GRÜN** — erste Kompilierung überhaupt; Compile-Fixes + 3 Logik-Bugs
  behoben (CaptureDriver pinchWasDown im Idle, HandMenuLogic Flanken-Toggle, Testdaten)
- **Selbsttest BESTANDEN**: JSON-Roundtrip + PDF-Beispiel (4,3 KB, valide: xref, Helvetica)
  — dialogfrei via `unity command script eval 'Hintergewerk.EditorTools.CliSelfTest.RunAll()'`
- **Projekt-Setup:** Android-Target, IL2CPP, ARM64, `de.bautoolz.hintergewerk`, **minSdk 32**,
  OpenXR-Loader (Android) + Meta-Quest-Feature aktiv (D27) — reproduzierbar via
  `Hintergewerk.EditorTools.ProjectSetup.Apply()` (Menü Hintergewerk → Projekt-Setup)
- `build.cmd`: Unity-Suche erweitert (`C:\Unity\Hub\Editor` als Hub-Secondary-Path), sort-Abhängigkeit entfernt
- Unity-CLI (1.0.0-beta.16) + `com.unity.pipeline` ermöglichen Agent-Fernsteuerung des Editors

## Deine ersten Schritte auf Windows (Tag 1, ~90 min, docs/06)

1. Unity 6 LTS + Module (Android Build Support, OpenJDK, SDK/NDK) — exakt eine Version fürs Projekt
2. Unity Hub: Projekt anlegen. **Achtung:** `unity/` ist nicht leer (enthält Assets/Hintergewerk).
   Trick: Projekt temporär daneben anlegen (3D URP), dann `Assets/Hintergewerk/` aus dem Repo
   ins neue Projekt kopieren, ProjectSettings+Packages im Repo committen (Projekthülle!).
3. Package Manager → **Meta XR All-in-One** (v207+, Wettbewerbsvorgabe) + **Meta XR Simulator**
4. XR Plug-in Management → Android → OpenXR + Quest-Feature; Hand Tracking aktivieren;
   Player: IL2CPP, ARM64; **Bundle-ID `de.bautoolz.hintergewerk`**
5. **Test Runner → EditMode → 51 Tests grün?** (erste echte Kompilierung — Fixes expected)
6. Menü **Hintergewerk → Kern-Selbsttest** → „bestanden" + **PDF-Beispiel erzeugen** → PDF prüfen
7. Erst dann: Adapter-TODOs gegen die installierte SDK-Version schließen
   (`Adapters/*.cs`, Referenz: developers.meta.com/vr/reference/unity/v207/)

## Danach (docs/07 + docs/11)

- Szenen 01_Smoketest + 02_AnkerTest bauen (Spec in docs/11, Controller existiert)
- Smoketest M0: APK → Dashboard-Channel „Competition" → Invite-URL → Quest → Hände sichtbar
- Anker-Test M1: 3 Runs Persistenz (Neustart / Headset ab / Raumwechsel) → GO/NO-GO
- Dann Capture-Flow (DrawController ist vorverkabelt), 05.–13.11. Feature-Politur,
  14.–15.11. Video (docs/08), 16.–17.11. Probe-Submission, 18.11. vormittags abgeben

## Mensch & Logins (Claude braucht KEINE davon — nur der Mensch bedient sie)

| Konto | Wofür |
|---|---|
| GitHub (Mathias) | Clone/Push dieses Repos |
| Unity ID | Hub + Personal-Lizenz |
| **Meta Developer Account** | Dashboard: App, Release-Channel „Competition", Upload — die Start-Programm-Mitgliedschaft hängt an diesem Konto |
| **Devpost** | Submission (dasselbe Konto, mit dem der Wettbewerb beigetreten wurde) |
| Meta/Quest-Konto am Headset | Developer Mode + Install über Invite-URL |
| lablab.ai | **Nicht nötig** — dieser Wettbewerb läuft über Devpost |

**Niemals teilen** (mit Claude oder sonstwo): Passwörter, Meta App Secret (wir nutzen
keine Graph-API — Dashboard-Upload ist manuell), private SSH-Keys, Tokens.

## Git-Workflow

- Repo ist privat. Vor Arbeit: `git pull`. Kleine Commits, deutsche Imperativ-Messages.
- Der Mac-Agent arbeitet parallel — Konfliktvermeidung: vorher pullen, eine Region pro Person.
- `build.cmd` (Repo-Root) baut die APK im Batch-Modus; `unity/build.log` ist das Log.
- Unity-generierte Dateien (Library/, Logs/) sind gegitignored — nur Quellen und Docs committen.

## Zieldefinition Erfolg

M0 (Pipeline) + M1 (Anker) ✅ → Capture-Flow < 30 s (M2) → Recall + Export (M3) →
Video ≤ 3 min (docs/08) → Submission komplett (docs/12) → **abgegeben vor 18.11., 21:00 CET**.
Scope-Leiter bei Zeitnot: docs/01 §5. Lieber ein runder Capture-Flow als vier halbe Features.

### Nachtrag 07.11. spät (Politur-Runde 1)
- **Scene-API-Wände** (D36): SceneWallProvider liest echte Wände (WALL_FACE), DrawController wählt die handnächste. Am Gerät: einmal Room Setup machen, dann zeichnet die App auf der echten Wand.
- **Detail-Panel** (D37): Blick aufs Objekt + Pinch → Panel (Typ, Konfidenz, Tiefe, Monteur, Datum, Foto). Panel-Kern unit-getestet; Auswahlgeste am Gerät prüfen (Checkliste docs/03 §6).
- Stand: **65/65 Tests grün**, APK final gebaut (Builds/hintergewerk.apk, alle Szenen 02→03→04→01, SceneSwitcher-Geste: linke Handfläche + Pinch).
