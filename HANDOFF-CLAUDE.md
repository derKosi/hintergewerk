# HANDOFF für Claude (Windows · Unity · RTX 3090)

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

## Update 05.11. (Windows-Session, Claude auf der 3090)

**Erledigt (alle Commits gepusht, 6000.6.8f1 fixiert):**
- **Monorepo-Entscheid (D24):** Unity-Projekt lebt im Repo unter `unity/` — Projekthülle
  (ProjectSettings + Packages) eingecheckt, erstellt per Unity-CLI aus URP-Blank (D24)
- **51/51 EditMode-Tests GRÜN** — erste Kompilierung überhaupt; Compile-Fixes + 3 Logik-Bugs
  behoben (CaptureDriver pinchWasDown im Idle, HandMenuLogic Flanken-Toggle, Testdaten)
- **Selbsttest BESTANDEN**: JSON-Roundtrip + PDF-Beispiel (4,3 KB, valide: xref, Helvetica)
  — dialogfrei via `unity command script eval 'Hintergewerk.EditorTools.CliSelfTest.RunAll()'`
- **Projekt-Setup:** Android-Target, IL2CPP, ARM64, `de.bautoolz.hintergewerk`, **minSdk 32**,
  OpenXR-Loader (Android) + Meta-Quest-Feature aktiv (D27) — reproduzierbar via
  `Hintergewerk.EditorTools.ProjectSetup.Apply()` (Menü Hintergewerk → Projekt-Setup)
- `Adapters/` liegt ausgeblendet als `Adapters~/` (D25) — Re-Enable + v207-TODOs nach SDK-Import
- `build.cmd`: Unity-Suche erweitert (`C:\Unity\Hub\Editor` als Hub-Secondary-Path), sort-Abhängigkeit entfernt
- Unity-CLI (1.0.0-beta.16) + `com.unity.pipeline` ermöglichen Agent-Fernsteuerung des Editors

**Offen / wartet:**
1. **Meta XR SDK v207** — nur noch via Unity Asset Store („Meta XR SDK", Produkt 9022845):
   „Add to my assets" braucht Mathias' Klick (EULA!) — Metas NPM-Registry ist tot (D26).
   Danach: Editor öffnen → `package add com.meta.xr.sdk.all` → `Adapters~` reaktivieren → TODOs gegen v207.
2. Szenen 01_Smoketest + 02_AnkerTest (Spec docs/11) — sobald SDK da ist (Meta XR Rig braucht es)
3. Smoketest M0 + Anker-Test M1 (Menschen-Anteil: Dashboard, Gerät — docs/07)
4. Erster Android-Build läuft als Pipeline-Check (SampleScene, ohne SDK-Rig)

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
