# Projektplanung — Hintergewerk

**Stand:** 31.10.2026 · **Deadline:** Mi 18.11.2026, 21:00 CET · **Budget:** 18 Tage, 1 Person + Agent

## 1. Ziel

Eingereichte, vollständige Submission beim Meta VR Start Developer Competition (Productivity / New Experience) mit einem ehrlichen, hands-first „Hintergewerk". Stretch: Top-3 im Productivity-Track.

Erfolg ist **nicht** „viel gebaut", sondern: **ein runder Flow, der auf dem Gerät läuft, über die Invite-URL für die Jury erreichbar ist, und in ≤ 3 Minuten Video überzeugt.**

**Vision geschärft (08.11., D39/D40):** Hintergewerk ist kein Raum-Scanner. Der Monteur hält einen
**Bauplan in der Hand** — Live-Top-Down-Sicht des aktuellen Raums — und trägt mit der freien Hand
Leitungen **hinter der Wand** ein. Das Ergebnis ist maschinenlesbar: **BIM-ready-Export**
(strukturiertes JSON + PDF-Grundriss), damit die Bauplanung damit weiterarbeiten kann.
Ehrlich gezeichnet: Der Bauplan ist **Output**, kein Scan-to-BIM (ADR-0003, ADR-0005).

## 2. Meilensteine

| # | Meilenstein | Bis | Akzeptanzkriterium (Beweis) |
|---|---|---|---|
| M0 | Pipeline steht | 02.11. | APK über Invite-URL auf der Quest installiert & gestartet (Screenrecording) |
| M1 | Anker hält | 02.11. | Objekt-Overlay überlebt App-Kill + Neustart deckungsgleich (Recording + Anchor-IDs) |
| M2 | Capture < 30 s | 07.11. | Person ohne Projektkenntnis zeichnet volles Objekt (Typ, Tiefe, Foto) ohne Erklärung |
| M3 | Recall + Export | 10.11. | Nach Neustart: Overlay mit Korridor; JSON + PDF auf Laptop lesbar |
| M4 | Politur | 13.11. | Zweiter Raum funktioniert; Cold Start < 15 s; keine Blocker auf Gerät |
| M5 | Abgabereif | 17.11. | Probe-Submission komplett durchlaufen; Video öffentlich; Formular 100 % |

## 3. Tagesplan

| Datum | Fokus | Output |
|---|---|---|
| Sa 31.10. | ✅ Start-Antrag bestätigt · Docs/Architektur (dieses Repo) | Gate 1 zu |
| So 01.11. | **Entfallen (keine Zeit)** — stattdessen autonom vorbereitet: kompletter SDK-freier C#-Kern (Datenmodell, JSON, Store, Snap, Korridor, Warnung, PDF-Export + Editor-Selbsttest), Setup-Guide, Smoketest-Protokoll, Video-/Text-Entwürfe | Code-Basis liegt bereit · Setup verlagert auf 02.11. |
| Mo 02.11. | Setup nach `06-setup-guide.md` (90 min Zeitbox) · **Smoketest M0** + **Anker-Test M1** nach `07-smoketest-protokoll.md` | GO/NO-GO für beide Gates |
| Di 03.11. | Integration: SDK-Adapter an `Core/Interfaces.cs` (OvrAnchorProvider, OvrHandSource, SceneWallProvider) · Entscheidungen fixieren | Architektur eingefroren |
| 03.–04.11. | Zeichnen: Pinch-Draw, Snap, Korridor · Objektmodell · Speichern/Laden | Kern-Capture technisch |
| 05.–07.11. | Hand-UI (Menü, Typwahl, Tiefe) · Beweisfoto · Objektliste · M2-Test | Capture rund |
| 08.–10.11. | Recall-Overlay · Fehlerfälle · Export · **M3** · Mitte-Scope-Check | Recall rund |
| 11.–13.11. | On-Device-Politur, Performance, zweiter Raum · **M4** | Stabilität |
| 14.–15.11. | Video (3 Szenen) · 500-Wörter-Text · Screenshots | Abgabematerial |
| 16.–17.11. | Puffer · **Probe-Submission** (M5) · Fixes | Abgabereif |
| **Mi 18.11.** | **Nur absenden — vormittags, hart vor 21:00 CET** | Submission ✅ |

## 3.1 Replanung 01.11.

Der Setup-Tag ist entfallen (Zeit). **Der Plan hält trotzdem**, weil die SDK-freie Arbeit vorgezogen wurde:
Datenmodell, Persistenz, Snap-/Korridor-/Warn-Mathematik und PDF-Export sind fertig geschrieben
(`unity/Assets/Hintergewerk/`, ungetestet bis zur ersten Kompilierung am 02.11.). Die Tage 03.–04.11.
("Objektmodell + Persistenz") verlieren dadurch ihre Kernlast — dort bleibt nur Adapter- und
Szenen-Arbeit. M2 (07.11.) bleibt realistisch. Neues Risiko R8: Setup-Tag-Verlust — mitigation:
Zeitbox 90 min + Abbruchkriterien im Guide + MCP-Fallback R6.

Zweite Vorbereitungsrunde (01.11., ohne Unity): **EditMode-Tests + asmdef-Struktur** (Runtime/Editor/Tests —
macht den Kern im Test Runner prüfbar), **AGENTS.md** (Konventionen für jede Session), **ADRs 0001–0004**
für die großen Weichen, **Szenen-Spec** (docs/11) und **Devpost-Feldvorlage** (docs/12).

Dritte Runde (01.11., weiter ohne Unity): **SDK-Adapter-Skelette** (OvrAnchorProvider/OvrHandSource/SceneWallProvider,
gegen Doku + Tutorial, TODOs für v207), **Anker-Test-Demo** (AnchorTestController, nur Interfaces),
**Capture-State-Machine + Undo-Stack** mit Tests, **Strings-Tabelle DE/EN** (Runtime/Localization),
**Normblatt-PDF** (1:20–1:200 + Schriftfeld), **JSON-Schema-Vertrag + Fixtures-Matrix**,
**Vorjahres-Analyse** (docs/13) und **Landingpage-Entwurf** (website/).

## 3.2 Replanung 08.11. — „Bauplan in der Hand" + BIM-ready (D38–D40, ADR-0005)

Anlass: Produktvision geschärft (Mathias) + Mitte-Scope-Check vorgezogen (war für 10.11. geplant, D11 → D38).
Zwei neue Elemente, beide ehrlich gegen die Deadline gerahmt — die Detail-Bewertung steht in der Tabelle oben (§1).

**Was neu gebaut wird:** Der `MapRenderer` projiziert bereits top-down (PDF-Pfad). **M4-Kern** ist,
dieselbe Projektion **live** als Textur auf einem Hand-Panel zu rendern — der Bauplan wächst mit,
während gezeichnet wird. Kein neuer Export-Pfad, keine neue Geometrie: Wiederverwendung.
Stretch (Scope-Leiter Sprosse 4): Panel als Zeichenebene (Ray aus dem Panel auf die Wandebene,
SnapEngine wiederverwendet) — Risko Winkelauflösung auf kleinem Panel.

**Meilensteine v2** (M0–M3 unverändert, M4/M5 geschärft):

| # | Meilenstein | Bis | Akzeptanzkriterium (Beweis) |
|---|---|---|---|
| M4 | Politur + **Bauplan in der Hand** | 13.11. | Panel in Hand zeigt den Raum top-down **live** (Objekt erscheint beim Confirm); zweiter Raum funktioniert; **Feature-Freeze** |
| M5 | Abgabereif | 17.11. | Probe-Submission komplett; Video zeigt: Zeichnen mit Bauplan → Recall-Korridor → Export (PDF + BIM-ready-JSON) |

**Timeline ab heute:**

| Datum | Fokus |
|---|---|
| Sa 08.11. | Replan (dieses Dokument) · **Headset-Block Mathias: M1 (3 Runs) + M2 + M3** (docs/07 — der einzige echte Engpass) |
| So 09.–Mo 10.11. | Gerät-Fehlerfälle + Fixes aus dem Headset-Block · Bauplan-Prototyp (Live-View) |
| 11.–13.11. | M4: Bauplan live + zweiter Raum + FoV-Audio · **Feature-Freeze 13.11.** |
| 14.–15.11. | Video (3 Szenen) + 500-Wörter-Text + Screenshots — Recording-Workflow aus YetAnotherDemoVR (`D:\Unity\Projects\YetAnotherDemoVR` — separates Projekt, nur Werkzeug-Wiederverwendung) |
| 16.–17.11. | Puffer · **Probe-Submission** (M5) · Invite-URL-Flow als Jury-Pfad nochmal testen |
| **Mi 18.11.** | **Nur absenden — vormittags, hart vor 21:00 CET** |

**Statusbilanz 08.11. (ehrlich):** Code dem Plan **voraus** (M3 vorgezogen, 65/65 Tests), Gerät-Verifikation
**hinterher** (M1 seit 02.11. überfällig — GO/NO-GO D6 noch offen). Der Engpass sind Headset-Stunden, nicht Code.

## 3.3 Replanung 10.11. — Headset-Block verschoben (Do 13.11. oder Wochenende)

Der geplante Headset-Block (war: 08.–10.11.) verschiebt sich auf **Donnerstag 13.11. oder das
Wochenende 14./15.11.** — Mathias' Kalender, nicht Plan-Drift. Konsequenzen (alle beherrschbar):

1. **Eine APK statt zwei:** Der Block läuft komplett mit der **APK 10.11.** (D41+D42+D43,
   103/103 Tests) — die Zwei-APK-Sequenz („erst 09.11. testen, dann Upgrade") entfällt.
2. **Test-Block = Dreh-Block:** Ein Durchlauf liefert BEIDES — Smoketest-Beweise (docs/07 §C–H)
   **und** das Video-Rohmaterial (docs/08 „Ein-Rutsch-Drehplan": jeder Schritt mappt auf einen
   Clip + Video-Beat). 14./15.11. werden dann zu **Schnitt-Tagen** (+ Szene-1-Realfilm, der
   braucht das Headset auch).
3. **Risiko verschärft — M1-Fallback vorbereitet:** Kippt M1 erst am Wochenende, bleibt kaum
   Laufweg für einen Umbau. Der R1-Fallback ist deshalb **jetzt entscheidungsreif** geschrieben
   (docs/07 „M1-NO-GO-Fallback"): Re-Lock über Scene-Wände statt blinder Anker, ~0,5–1 Tag
   Code, Story-Anpassung im Video/Text. Trigger: M1 Run 1+2 fail.
4. **Freeze 13.11. bleibt** — er friert CODE ein, nicht Verifikation. D43 ist mit den
   103/103 Tests freeze-reif; der Gerätetest bestätigt danach nur noch. Kein weiteres Feature
   vor dem Block (Scope-Leiter gilt).
5. **Invite-URL (Jury-Pfad) bleibt der erste Schritt im Block** — falls der Flow klemmt, bleibt
   für Support/Puffer mehr Zeit als bei einem Block am 17.11.

## 4. Risiko-Register

| # | Risiko | Impact | Gegenmaß | Entscheidung bis |
|---|---|---|---|---|
| R1 | Anker persistiert nicht (Neustart/Raumwechsel) | Kern | Fallback: Re-Lock über Scene Mesh/Ebenen; Story anpassen | 02.11. |
| R2 | Invite-URL-Flow bricht | Abgabe | Früh testen; Devpost-/Meta-Support; Forum | 02.11. |
| R3 | Beweisfoto: Passthrough-Aufnahme aus App API-beschränkt | mittel | App-Render-Snapshot als Beweisfoto; System-Screenshot für Demo | 06.11. |
| R4 | PDF-Export zu teuer | mittel | Eigener Mini-PDF-Emitter (Linien + Text); Fallback PNG + In-App-Legende | 10.11. |
| R5 | Zeitnot ab 10.11. | hoch | Scope-Leiter (§5) | 10.11. |
| R6 | Unity-MCP-Anbindung klappt nicht | Tempo | Fallback: reine Code-/Asset-Übergabe, Mathias importiert | 01.11. |
| R7 | Hand-Tracking zu unpräzise zum Zeichnen | mittel | Grammatik ist dagegen gebaut (Snap/Raster/Magnet); notfalls Zwei-Punkt-Methode | 04.11. |
| R8 | Setup-Tag 01.11. entfallen → Termindruck | mittel | SDK-freier Kern vorgezogen (fertig), Guide mit Zeitbox + Abbruchkriterien | 02.11. |

## 5. Scope-Leiter v2 (Mitte-Scope-Check 08.11., D38 — bei Zeitnot in dieser Reihenfolge streichen)

1. ~~Agenten-Vorschlag (Agentic Interaction Award)~~ — **gestrichen 08.11.** (nicht gebaut; D10 bleibt NO-GO)
2. ~~4. Objektklasse „Nicht-bohren-Zone" (Fußbodenheizung)~~ — **gestrichen 08.11.** (Typ bleibt im Farbkanon D31)
3. ~~Sprachnotiz~~ — **gestrichen 08.11.** (nicht gebaut)
4. Bauplan als Zeichenebene (auf dem Panel tippen) — Stretch; wenn es hakt: Bauplan bleibt Live-Ansicht
5. PDF-Export → PNG-Export — bleibt **unberührt**: PDF ist gebaut und getestet, Wegwerfen spart nichts
6. Video-Fokus auf Zeichnen (mit Bauplan) + Export; Recall-Korridor als Beleg zeigen

**Nie streichen:** Hands-first-Kernflow · Ehrlichkeitsregel/Konfidenz · Speichern/Laden · Invite-URL-Test ·
BIM-ready-Export (JSON + PDF) als Pitch-Kern.

## 6. Human-Punkte (nur Mathias)

Zeitbudget, Zeitboxen und Ist-Tracking: **docs/14-timesheet.md**.

1. ~~Start-Programm-Antrag~~ ✅ 31.10. bestätigt
2. 01.11.: Unity 6 + Meta XR SDK + MCP-Server + Repo aufsetzen (Checkliste §7)
3. Quest 3: Developer Mode, USB, Build installieren
4. 02.11.: Invite-URL auf dem Gerät öffnen (Test-Install)
5. 14.–15.11.: Video aufnehmen (Gerät/Screen)
6. 18.11.: Devpost-Submission absenden + Bestätigung sichern

## 7. Setup-Checkliste 02.11. (morgens, Zeitbox 90 min)

- [ ] Unity 6 (LTS) + Unity Hub, Cloud-Projekt angelegt
- [ ] Meta XR SDK All-in-One importiert (lt. Wettbewerbsseite v207+)
- [ ] Projekt auf Android/OpenXR/Quest konfiguriert, Hand-Tracking aktiv
- [ ] AI-Assistant-Paket installiert, MCP-Server läuft, Editor verbunden
- [ ] Repo initialisiert (GitHub oder Forgejo — Entscheidung D7), Unity-.gitignore
- [ ] Devpost-Draft angelegt; „Competition"-Release-Channel im Dashboard erstellt
- [ ] XR Simulator: erste Szene läuft (Würfel + Hände)

Optional evaluieren: Meta VR CLI (`npx metavr@latest init`) — offizielles AI-Tooling für die Plattform.
