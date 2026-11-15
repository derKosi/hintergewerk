# Video-Voiceover — Entwurf v1 (Englisch, Ziel-Länge 2:40)

> Sprache: Englisch (internationale Jury). Langsam und klar sprechen; Tech-Begriffe sauber
> aussprechen („anchor", „passthrough"). Musik: frei (Meta/YouTube Audio Library), leise unter dem VO.
> Regel: Die ersten 40 Sekunden müssen allein funktionieren (First-Five-Minutes-Award).
> Aussprache im VO: einmal langsam einführen („HIN-ter-guh-verk — the work behind the surface"), danach normal weitersprechen.

## Szene 1 — The golden hour (0:00–0:40)

**Bild:** Wand offen (Trockenbau), Monteur vor der Wand, Quest auf. Hände sichtbar.

- 0:00 — *(Bohrer nähert sich einer geschlossenen Wand, Schnitt, STOPP)*
  **VO:** „Every year, drill bits die in walls they never understood."
- 0:06 — *(Schnitt zur offenen Wand, Leitung sichtbar)*
  **VO:** „But the one moment a wall tells the truth — is the moment it's open. We capture that moment."
- 0:14 — *(Hand flach → Menü, radialer Typ-Wahl „Elektro")*
  **VO:** "Hintergewerk — the work behind the surface. No controllers, no tablet — just hands."
- 0:22 — *(Pinch am Startpunkt: Snap-Puls; Linie entlang der Leitung ziehen, Punkte rasten ein)*
  **VO:** „Draw the run along the open wall. Snap points absorb hand tremor — precision by design, not by luck."
- 0:32 — *(Zwei-Hand-Distanz → „4,5 cm" schwebt zwischen den Händen; Foto-Anhang)*
  **VO:** „Two hands set the depth. A photo proves it. Thirty seconds, and this cable exists — forever."

## Szene 2 — Months later (0:40–1:40)

**Bild:** Dieselbe Wand, geschlossen, frisch gespachtelt. Neue Session (Overlay ein).

- 0:40 — *(App-Start, Overlay blendet ein: Korridore leuchten)*
  **VO:** „Months later. Same wall — new session. The overlay returns, anchored where we left it."
  *(Ehrlichkeits-Banner: „captured Nov 2 · 14 days · 9 app restarts")*
- 0:50 — *(Hand nähert sich dem Korridor: gelb → rot, Audio-Ping)*
  **VO:** „Not a thin line — a safety corridor. As your hand approaches, it escalates. The drill stops before the pipe does."
- 1:05 — *(Blick + Pinch auf Objekt → Detail-Panel: Foto-Zustand, Tiefe, Konfidenz, Monteur, Datum)*
  **VO:** „Look and pinch: the photo state, the depth, who documented it, and when. Estimated until measured — and we say which. We never guess silently."
- 1:25 — *(Geist-Modus-Moment falls gebaut: Möbel-Ghost wegschieben)*
  **VO:** „Furniture in the way? Push the ghosts aside — the wall is yours again."

## Szene 2b — The plan grows in your hand (0:52–1:05, Bauplan-Beat, D39)

**Bild:** Während Szene 2 läuft: Bauplan-Panel an der linken Hand im Blick — der Live-Top-Down-Plan
des Raums, Korridore in Typfarben. Beim Bestätigen eines neuen Objekts erscheint die Linie **live**
im Plan (Simulator-Beweis: `docs/beweise/2026-11-08-simulator-m3/m13_zweiRaeume.png`).

- **VO:** „And while you draw — the floor plan grows in your other hand. Live. Every corridor you
  document, every room you open — BIM-ready when you leave the site."
- *(Ehrlichkeits-Regel: „BIM-ready" ≠ „BIM" — kein IFC-Claim, siehe D40. Panel-Statuszeile zählt
  verortbar/nicht verortbar sichtbar mit.)*
- **Dreh-Quelle:** Szene 04 (am Gerät: Room Setup vorher). Checkliste: docs/07 §G.

## Szene 3 — The proof (1:40–2:30)

**Bild:** Objektliste; dann Export: PDF-Grundriss füllt Panel.

- 1:40 — *(Objektliste scrollen: Typ, Raum, Konfidenz)*
  **VO:** „Every object, every room — the memory grows with the house."
- 1:50 — *(PDF-Export: Grundriss mit Legende, Maßstab, Korridoren)*
  **VO:** „One gesture exports a floor plan with legend and scale — the document your builder accepts, and the next firm inherits."
- 2:05 — *(Falls gebaut: Agenten-Trasse-Vorschlag; sonst „What's next"-Folie)*
  **VO:** „What's next: scanner results join the memory, whole houses, staircases — and agents that suggest new routing around what's already there."
- 2:18 — *(Endkarte: Logo + Tagline)*
  **VO:** „Hintergewerk. What your walls once showed, they will always remember."

## Dreh-Checkliste (14.–15.11.)

- [ ] Gerät: Quest 3, gutes Licht, Passthrough hell genug
- [ ] On-Screen-Texte groß genug für Kompression (min. 28 pt equivalents)
- [ ] Session-Banner „X days / Y restarts" echt sein — aus dem Store-Log ablesen, nicht schätzen
- [ ] Keine Copyright-Musik, keine fremden Marken/logos im Frame (Werkzeug de-branden)
- [ ] Rohmaterial pro Szene 3 Takes; 3-Minuten-Grenze: 2:40 anpeilen (Puffer)

## Ein-Rutsch-Drehplan (10.11., Replan docs/01 §3.3) — Test-Block = Dreh-Block

Der Headset-Block (Do 13.11. oder Wochenende) läuft mit der **APK 10.11.** und liefert in EINEM
Durchlauf alle Smoketest-Beweise UND das Video-Rohmaterial. **Meta-Taste halten → „Video
aufnehmen" in kurzen Clips pro Schritt** (nicht eine lange Rolle) — Passthrough wird mit
aufgenommen. USB bleibt dran (logcat + Dateien holen). Danach Clips ziehen:
`adb shell ls /sdcard/Movies/Captures` → `adb pull <pfad> video_material/`.

| # | Schritt (Protokoll) | Dauer | Aufnehmen → landet als | Beat |
|---|---|---|---|---|
| 1 | Invite-URL-Flow (§C/D) | 20 min | Clip „Install über Link" (Beweis M0-Weg; optional B-Roll) | — |
| 2 | **M1 3× Anker-Persistenz** (§M1) | 15 min | je Run 1 kurzer Clip (Würfel deckungsgleich nach Neustart) | Ehrlichkeits-Banner „X days · Y restarts" (0:40) |
| 3 | **M2 Capture** (§F) mit Stoppuhr | 5 min | 2–3 Takes „Menü → Zeichnen → Snap → Tiefe → Confirm" | Szene 1: 0:14–0:40 |
| 4 | **M3 Recall** (§E): Korridore, Eskalation, Export | 10 min | Clips: Overlay-Einblendung · Hand→Bernstein→Rot · Export-Pinch | Szene 2: 0:40–1:05 + Szene 3: 1:50 |
| 5 | **Detail-Panel** (docs/03 §6): Blick+Pinch | 5 min | Clip „Blick + Pinch → Panel mit Foto/Konfidenz" | Szene 2: 1:05–1:25 |
| 6 | **Bauplan + Räume** (§G + §H, D43): zwei Räume zeichnen, Plan wächst, Export je Raum | 10 min | Clips: „Plan wächst in der Hand" · „zweiter Raum erscheint im selben Plan" · PDF-Stapel | Szene 2b: 0:52–1:20 (**Kopf-Feature, 2–3 Takes**) |
| 7 | Cold Start (Stoppuhr < 15 s) | 2 min | Clip App-Icon → Statuszeile (M4-Beweis) | — |
| 8 | **Szene 1 Realfilm** (Handy-Kamera, separates Setup): Bohrer vor geschlossener Wand (STOPP-Moment) · offene Wand mit Leitung · Kopf/Hände im Passthrough | 30 min | 3 Takes je Einstellung | Szene 1: 0:00–0:14 (kritischer Pfad!) |
| 9 | Screenshots für Devpost (docs/12-Liste, 7 Items) | 10 min | OS-Screenshots (Meta-Taste kurz) | Submission-Bilder |

**Gesamt: ~1 h 40 min.** Danach am Laptop: `adb pull` (Clips + `hgw_export/` JSON/PDF),
logcat-Transkript sichern (M1-UUIDs, `[SceneWallProvider]`-Zeilen = Beweise für docs/07).

Wenn der Block auf **Donnerstag** fällt: 14./15.11. = nur Schnitt + VO-Einsprechen (docs/14
Block F schrumpft von 2,5 h auf ~1,5 h). Wenn am **Wochenende**: Montag 16.11. = Schnitt-Tag,
Probe-Submission 17.11. bleibt — knapp, aber machbar.

## Fertige Video-Assets (10.11., Agent)

In `docs/video-assets/` (1920×1080, PNG): **Endkarte** (Logo-Wortmarke + Tagline, Beat 2:18) ·
**„What's next"-Folie** (Beat 2:05, passt zum VO in Szene 3) · **Ehrlichkeits-Banner-Vorlage**
(„captured … · … days · … restarts", Werte am Drehtag echt einsetzen — docs/08-Regel). Stil folgt
`website/style.css` (dunkel, Papier-Ton).

## Animatic-Trockenübung (09.11., Agent) — Beats × vorhandene Beweis-Fotos

Ziel: Der Schnitt ist VOR dem Dreh montierbar — jeder Beat hat ein Standbild aus den
Simulator-Beweisen, der Dreh ersetzt es nur durch echtes Passthrough-Material. Gaps sind
explizit markiert (das NUR-am-Gerät-Gefilmte). Grundlage: `docs/beweise/`.

| Beat | Zeitslot | Standbild (Animatic-Frame) | Gap: nur am Gerät drehbar |
|---|---|---|---|
| 0:00 Bohrer/Wand-Stop | 0:00–0:06 | ⟨Echtfilm, kein Ersatz⟩ | Realfilm: geschlossene Wand, Bohrer |
| 0:06 offene Wand | 0:06–0:14 | ⟨Echtfilm⟩ | Realfilm: offene Wand, Leitung |
| 0:14 Menü + Typ-Wahl | 0:14–0:22 | ⟨Echtfilm; Menü-Logik bewiesen (Tests)⟩ | Hand-Fläche + radiales Menü am Gerät |
| 0:22 Zeichnen + Snap | 0:22–0:32 | `s03_plan_zeichnen_mit_umriss.png` (Raumkontext) | Zeichnen in echter offener Wand (Passthrough!) |
| 0:32 Tiefe + Foto | 0:32–0:40 | ⟨Echtfilm; Panel-Tests bewiesen⟩ | Zwei-Hand-Distanz „4,5 cm" + Foto-Puls |
| 0:40 Overlay-Return | 0:40–0:50 | `m7_korridore.png` (Korridore sichtbar) | Overlay in frisch gespachtelter echter Wand |
| 0:50 Eskalation | 0:50–1:05 | `m7_eskalation.png` (Bernstein→Signalrot) | Hand-Näherung + Audio-Ping (FoV-Pan am Gerät hören!) |
| 0:52 Bauplan-Beat (2b) | 0:52–1:05 | `s04_bauplan_alle_raeume.png` (Plan + Umriss + 2 Räume) | Plan in echter Hand, echter Umriss aus Room Setup |
| 1:05 Detail-Panel | 1:05–1:25 | ⟨Panel-Unit-Beweise⟩ — am Gerät drehen | Blick+Pinch Auswahl (docs/03 §6-Checkliste) |
| 1:25 Geist-Modus | 1:25–1:40 | ⟨GESTRICHEN, D38⟩ — Beat streichen oder „What's next" ziehen | — |
| 1:40 Objektliste | 1:40–1:50 | ⟨Echtfilm/JSON⟩ | Objektliste im Gerät-UI (falls gebaut; sonst Folie) |
| 1:50 PDF-Export | 1:50–2:05 | PDF-Beispiel (Menü „PDF-Beispiel erzeugen", 4,3 KB) + Export-Logs „k Wand-Segmente" | Export-Pinch am Gerät; PDF auf Laptop daneben filmen |
| 2:05 What's next | 2:05–2:18 | Folie (kein Foto nötig) | — |
| 2:18 Endkarte | 2:18–2:30 | Folie: Logo + Tagline | — |

**Erkenntnis aus der Trockenübung:** 6 von 13 Beats haben schon jetzt brauchbare Frames aus den
Beweisen — der kritische Pfad ist Szene 1 (alles Realfilm/Passthrough, kein Simulator-Ersatz).
Szene 2b ist das am besten belegte Feature (4 Fotos). Zeitslot „Geist-Modus" frei → Szene 2b
darf 0:52–1:20 wachsen („plan grows" hat B-Roll-Verdopplung verdient: Zeichnen + Plan im Split).
