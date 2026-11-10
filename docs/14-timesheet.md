# Timesheet — Mathias (Menschen-Minuten bis zur Abgabe)

Zweck: Dein Zeitbudget bis zum **18.11.2026, 21:00 CET** — in Zeitboxen geschnürt. Prinzip: Der Agent liefert alles Vorarbeit; du machst nur die HUMAN-Punkte. Jeder Block ist unabhängig „einquetschbar" — außer Block B vor C/D.

**Gesamtbudget: ca. 8,5 h deiner Zeit** (davon 2,5 h Video).

## Ist (erledigt)

| Datum | Task | Zeit | Status |
|---|---|---|---|
| 31.10. | Start-Programm-Antrag | 5 min | ✅ bestätigt |
| 31.10. | Name + Strategie-Entscheidungen (D1–D23) | ~45 min | ✅ |

## Blöcke (Soll)

> **Update 10.11. (Replan docs/01 §3.3):** Die Blöcke C+D+E laufen gebündelt als **EIN
> Headset-Block (~1 h 40 min)** am **Do 13.11. oder Wochenende** — mit der APK 10.11.
> (D43 drin) und parallel als Video-Dreh (docs/08 „Ein-Rutsch-Drehplan"): Jeder
> Testschritt erzeugt zugleich den Clip fürs Video. Block F wird dadurch zum
> Schnitt-Tag (~1,5 h statt 2,5 h).

### Block A — Repo anlegen + Push · ✅ ERLEDIGT (04.11. — Mathias hat selbst gepusht)
1. github.com → privat Repo `hintergewerk` (ohne README)
2. Settings → SSH keys → Key aus `docs/10 §B` einfügen
3. Repo-URL an den Agent → Agent pusht
**Ergebnis:** 3090 kann pullen, CI-Basis steht.

### Block B — Setup-Tag · 90 min · Zeitbox! · docs/06
Unity 6 + SDK + MCP + Quest Dev-Mode + Kern-Ordner rein. Am Ende: Selbsttest grün, Test Runner grün.
**Abbruchkriterien stehen im Guide — nicht dran kleben bleiben.**
Mac-Arbeitskopie liegt bereit: `~/Documents/Dev/hintergewerk` (Git-Sync vom Agent); die 3090 klont später von GitHub.

### Block C — Dashboard + Smoketest M0 · 45 min · docs/07 §A–D
Dashboard-App anlegen → Channel „Competition" → APK-Upload → Invite-URL → Install auf Quest → Screenrecording als Beweis.

### Block D — Anker-Test M1 · 30 min · docs/07
3 Runs (Neustart, Headset ab/auf, Raumwechsel) + UUID-Logs als Beweis. **GO/NO-GO für das Produktkonzept.**

### Block E — Feature-Tage 05.–13.11. · täglich 20–30 min
Gerätests nach Checkliste (docs/03 §6). Der Agent baut via MCP — du testest und sagst, was sich falsch anfühlt. Probe-PDF am 10.11. anschauen.
**Neu 07.11.:** M2 (Capture-Zeit) + M3 (Recall/Export) sind gebaut — Gerätetests nach **docs/07 §E/F**; Szenenwechsel am Gerät ohne adb (linke Handfläche + Pinch).

### Block F — Video 14.–15.11. · 2,5 h · docs/08
Dreh-Checkliste: 3 Takes pro Szene, Gerät-Footage zuerst, Musik lizenzfrei. Englischer VO ist vorgeschrieben (docs/08, mit Aussprache-Notizen).

### Block G — Abgabe 17.–18.11. · 60 min
17.11.: Probe-Submission komplett (docs/12). 18.11. vormittags: final absenden + Bestätigungs-E-Mail sichern.

## Notfall-Modus („ich habe nur …")

| Verfügbare Zeit | Was du machst |
|---|---|
| 5–15 min | ~~Block A~~ erledigt — jetzt: **Block B** starten (Setup-Tag) |
| 30 min | Block A + B-Schritt 1 (Unity-Download starten) |
| 45–60 min | Block B komplett ODER C+D kompakt hintereinander |
| 2 h | Block B + C + D an einem Tag = alle GO/NO-GOs erledigt |

## Offene Abhängigkeiten (der Agent wartet auf dich)

| Wartet auf | Blockt |
|---|---|
| ~~Repo-URL (Block A)~~ ✅ erledigt | — |
| Unity + MCP läuft (Block B) | Szenen-Bau, Adapter-Verifikation |
| Unity + MCP läuft (Block B) | Szenen-Bau via MCP, Adapter-Verifikation |
| Quest-Tests (Block C/D) | GO für Capture-Flow-Finalisierung |

## Optionale Agent-Tasks (auf Bestellung, ohne dich)

- CI-Workflow (GitHub Actions) für den SDK-freien Kern
- Impressum/Datenschutz-Feinschliff für die Website (Draft existiert)
- Landingpage-EN-Leicht-Variante ergänzen
