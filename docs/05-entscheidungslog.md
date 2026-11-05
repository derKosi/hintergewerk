# Entscheidungslog

Format: eine Entscheidung pro Zeile — **Was · Warum · Konsequenz.** Neue Entscheidungen unten anfügen (aufsteigend datiert), nichts löschen.

| Datum | ID | Entscheidung | Status | Kontext / Konsequenz |
|---|---|---|---|---|
| 31.10. | D1 | Track: **Productivity** | ✅ | Kreative Tools / Effektivität — passt zum Produktkern |
| 31.10. | D2 | Division: **New Experience** | ✅ | Projekt entsteht komplett im Fenster ab 24.09. — regelkonform |
| 31.10. | D3 | Start-Mitgliedschaft beantragt | ✅ | Bestätigungsmail erhalten — Gate 1 zu |
| 31.10. | D4 | Stack: **Unity 6 + Meta XR SDK** | ⏳ GO 01.11. | Anker-Persistenz ist die kritische Annahme → direkt nativ, kein WebXR-Umweg. GO nach erfolgreichem Setup |
| 02.11. | D5 | Smoketest Invite-URL-Flow | ⏳ | GO/NO-GO; bei NO-GO sofort Devpost-/Meta-Support |
| 02.11. | D6 | Anker-Persistenz | ⏳ | GO → Produkt wie geplant. NO-GO → Re-Lock-Fallback über Scene Mesh, Story anpassen |
| 03.11. | D7 | Repo (GitHub/Forgejo) + Lizenz | ⏳ | Privat reicht; keine OSS-Pflicht |
| 06.11. | D8 | Beweisfoto-Ansatz | ⏳ | Passthrough-API verifizieren; Fallback App-Render-Snapshot |
| 07.11. | D9 | 4. Objektklasse „Nicht-bohren-Zone" | ⏳ | Nur wenn 05.–07. glatt läuft |
| 10.11. | D10 | Agenten-Vorschlag bauen? | ⏳ | Nur bei stabilem Recall (Agentic Interaction Award) |
| 10.11. | D11 | Mitte-Scope-Check | ⏳ | Video-Fokus festlegen (Scope-Leiter in 01-projektplanung §5) |
| 10.11. | D12 | PDF vs. PNG-Export | ⏳ | Nach Aufwand des PdfEmitters |
| 13.11. | D13 | Finale Tagline (Name entschieden, siehe D19) | ⏳ | Kandidaten in 04-submission §4 |
| 14.11. | D14 | Video-Framing | ⏳ | Sitzend-Szenen wo möglich, „X Tage / Y Neustarts"-Ehrlichkeit |
| 31.10. | D15 | Raumerfassung: **Scene API, Grundriss als Export** — kein Scan-to-BIM | ✅ | Blueprint ist Output, nicht Input · Details: 02-architektur.md §7 |
| 31.10. | D16 | **Geist-Modus + Möbel-Push** als Stretch nach M2; Onboarding über System-Room-Setup | ⏳ Gate 07.11. | Scan-Erlebnis = OS-Flow, kein eigenes Scan-Modul (kein LiDAR, keine Semantik) · Details: 02-architektur.md §7.1 |
| 01.11. | D17 | SDK-freier C#-Kern vorgezogen und fertig geschrieben; Meta-SDK nur noch in dünnen Adaptern hinter `Core/Interfaces.cs` | ✅ | Setup-Tag-Verlust aufgefangen · Kompilierung + Selbsttest am 02.11. · Code ist ungetestet bis dahin |
| 01.11. | D18 | Windows-3090-Kiste als **sekundärer Build-Runner** (Git + SSH + Batch-Build); **Unreal verworfen**; Mac bleibt Primärmaschine (AutoClaw + MCP) | ⏳ Setup 02.–03.11. | Anleitung: 10-windows-buildserver.md · Fernsteuerung (Parsec/RDP) optional für Mathias · M0/M1 hat Vorfahrt |
| 01.11. | D19 | Produktname: **Hintergewerk** (Arbeitstitel „Hausgedächtnis" verworfen) | ✅ | Oberflächen-neutral (Wand/Decke/Boden/Garten), freie .de (noch nicht registriert), im Englischen als „the work behind" lesbar · Repo, Namespace und Docs umbenannt |
| 01.11. | D20 | UI-Sprache: **DE primär**, zentrale String-Tabelle (`Runtime/Localization`), Video + Submission **EN** | ✅ | Jury liest EN in der Submission, Handwerker arbeiten mit DE — Strings zentral statt im Code verstreut |
| 01.11. | D21 | Video-Opener: **Problem-Beat vor Feature-Demo** (aus Vorjahres-Analyse, docs/13) | ✅ | Erste 40 s: Bohrer-Szene + goldene Stunde — Emotion vor Funktion, wie bei den Referenzprojekten |
| 01.11. | D22 | Editor-First-Iteration: **Mocks + CaptureDriver** — kompletter Flow im Editor testbar, das Gerät bleibt die Wahrheit (M0/M1) | ✅ | Sparzeit am kritischen Pfad; Mocks in Runtime/Mocks, SDK-frei |
| 01.11. | D23 | Promo-Site **4-sprachig**: DE · EN · DE-leicht (Leichte Sprache) · EN-simple — mit Sprach-Umschalter und Tagebuch | ✅ | Leichte Sprache = echter Accessibility-Hebel (Special Award) + hilft nicht-techischen Besuchern; statisch, deploy auf bau-toolz |
| 05.11. | D24 | **Monorepo**: Unity-Projekt lebt im Repo unter `unity/` (nicht `D:\Unity\Projects`); Projekthülle (ProjectSettings + Packages) eingecheckt, Editor **6000.6.8f1** fixiert | ✅ | build.cmd-Pfad (`unity/`) hält · Mac-Arbeitskopie öffnet `<repo>/unity` · Library/Temp/Logs/UserSettings gegitignored · erstellt per Unity-CLI (URP-Blank) |
| 05.11. | D25 | `Adapters/` vorerst ausgeblendet als `Adapters~/` (Unity ignoriert `~`-Ordner), bis Meta XR SDK v207 installiert ist | ✅ | Erster Import kompiliert sonst nicht (Safe Mode) · **Re-Enable:** `git mv Adapters~/Adapters.meta Adapters.meta` + `git mv Adapters~ Adapters`, dann Adapter-TODOs gegen v207 schließen |
| 05.11. | D26 | Meta XR SDK-Bezug: **Metas offizielle NPM-Registry `npm.developer.oculus.com`** (scoped `com.meta`) → `com.meta.xr.sdk.core` **207.0.1**; Asset Store nur noch für das All-in-One-Wrapper-Paket (nicht benötigt) | ✅ 06.11. | Kein Menschen-Klick nötig · Core deckt OVR*/Anker/Hände ab · Adapter gegen v207-API umgeschrieben und kompiliert (06.11.) |
| 05.11. | D27 | **minSdk 32** (statt 29 lt. docs/06) + Editor-Werkzeuge `CliSelfTest` (dialogfreier Selbsttest) und `ProjectSetup` (Android/OpenXR reprozierbar) | ✅ | Meta-Quest-OS-Vorgabe · Selbsttest per `unity command script eval 'Hintergewerk.EditorTools.CliSelfTest.RunAll()'` fahrbar · 51/51 Tests + JSON-Roundtrip + PDF (4.272 Bytes) bestanden am 05.11. |
| 06.11. | D28 | Szenen 01_Smoketest + 02_AnkerTest gebaut: **SDK-Prefabs** (`OVRCameraRig` + `OVRCustomHandPrefab_L/R` unter den Hand-Anchors) statt Handbau; StatusText als **TextMesh** statt TMP (kein Essentials-Import nötig — Funktionsumfang für M0 identisch) | ✅ | docs/11-Spec erfüllt (TMP-Abweichung dokumentiert) · Adapter-Verdrahtung per SerializedObject in Szene 02 · beide Szenen in der Build-Liste (01 zuerst) |
| 06.11. | D29 | **Meta XR Simulator**: Binaries liegen dem Core-SDK NICHT bei — Installer lädt MSI (435 MB) nach `Downloads`; Unity-Auto-Download schlägt hier fehl (Datei-Lock?) | ⏳ Menschen-Klick | MSI liegt bereit: `_hgw_unity_tmp/sim/meta_xr_simulator.msi` (v207.1) — Doppelklick genügt, dann Menü `Window/Meta/Meta XR Simulator/Activate` · kein Blocker für M0/M1 (die laufen am Gerät) |
