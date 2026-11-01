# Hintergewerk

> **Das Wissen hinter der Oberfläche.** Was einmal sichtbar war, bleibt wiederfindbar —
> räumlich verankert, mit Tiefe, Typ, Zeitstempel und Monteur.
>
> Englisch gelesen: *the work behind the surface* — der Name trägt in beiden Sprachen.

| | |
|---|---|
| **Wettbewerb** | [Meta VR Start Developer Competition 2026](https://start-developer-competition-26.devpost.com/) |
| **Track / Division** | Productivity · New Experience |
| **Abgabe** | **Mi, 18.11.2026, 12:00 PST = 21:00 CET** (hart, danach gesperrt) |
| **Team** | Mathias — Gerät, Dashboard, Video, Submission · AutoClaw — Code, Szenen (Unity MCP), Doku |

## Die Ehrlichkeitsregel (trägt das ganze Produkt)

Wir behaupten **nicht**, durch Wände zu sehen. Wir erinnern, was dokumentiert wurde.
Jedes Objekt trägt eine Konfidenz — `gemessen` / `geschätzt` / `Sucher`.
**„Nicht gefunden" heißt nie „nicht vorhanden".**

## Status (täglich aktualisieren)

| Gate | Frist | Status |
|---|---|---|
| Start-Programm-Mitgliedschaft | 31.10. | ✅ **bestätigt** |
| SDK-freier C#-Kern vorbereitet | 01.11. | ✅ **geschrieben** (Kompilierung folgt 02.11.) |
| Setup Unity + SDK + MCP + Repo | 02.11. | ⏳ |
| Smoketest: APK → Channel → Invite-URL → Quest | 02.11. | ⏳ |
| Anker-Persistenz über Neustart | 02.11. | ⏳ |
| Capture-Flow < 30 s pro Objekt | 07.11. | ⏳ |
| Recall-Overlay + Export (JSON/PDF) | 10.11. | ⏳ |
| Politur, zweiter Raum | 13.11. | ⏳ |
| Video + 500-Wörter-Text + Screenshots | 15.11. | ⏳ |
| Probe-Submission komplett | 17.11. | ⏳ |
| **Abgabe** | **18.11. vormittags** | ⏳ |

## Dokumentation

| Doc | Inhalt |
|---|---|
| [docs/01-projektplanung.md](docs/01-projektplanung.md) | Timeline, Meilensteine, Risiken, Scope-Leiter, Human-Punkte |
| [docs/02-architektur.md](docs/02-architektur.md) | Stack, Module, Datenmodell, Persistenz, Export, Raumerfassung (D15/D16) |
| [docs/03-interaktion.md](docs/03-interaktion.md) | Hand-Grammatik, Snap/Korridor, Flows, Testkriterien |
| [docs/04-submission.md](docs/04-submission.md) | Einreichungs-Checkliste, Video-Storyboard, Text-Gerüst, Judging-Map |
| [docs/05-entscheidungslog.md](docs/05-entscheidungslog.md) | Alle Entscheidungen, datiert, mit Konsequenz |
| [docs/06-setup-guide.md](docs/06-setup-guide.md) | Klickpfad-Setup 02.11.: Unity, SDK, MCP, Quest, Repo (Zeitbox 90 min) |
| [docs/07-smoketest-protokoll.md](docs/07-smoketest-protokoll.md) | M0-Pipeline + M1-Anker-Persistenz: Schritte, Beweise, Fallbacks |
| [docs/08-video-voiceover.md](docs/08-video-voiceover.md) | Video-Sprechtext v1 (EN, 2:40) + Dreh-Checkliste |
| [docs/09-submission-text-entwurf.md](docs/09-submission-text-entwurf.md) | Devpost-Text v1 (EN, ~430 Wörter, bis Feature-Freeze pflegen) |
| [docs/10-windows-buildserver.md](docs/10-windows-buildserver.md) | Windows-Zweitmaschine (3090): Git, OpenSSH, Remote-Build, Fernsteuerung, Security |
| [docs/11-szene-spec-smoketest.md](docs/11-szene-spec-smoketest.md) | Exakte Szenen-Spec für M0/M1: Hierarchy, Verhalten, Pass-Kriterien |
| [docs/12-devpost-felder.md](docs/12-devpost-felder.md) | Formularfelder fertig zum Einfügen (17.11.) + Hand-Interactions-Text |
| [docs/13-vorjahres-analyse.md](docs/13-vorjahres-analyse.md) | Was die Vorjahres-Referenzen gewinnen ließ — Muster + Ableitungen für Video/Text |
| [website/index.html](website/index.html) | Promo-Site **4-sprachig** (DE / EN / DE-leicht / EN-simple) + Tagebuch, deploy-ready |
| [docs/adr/](docs/adr/README.md) | ADRs 0001–0004: die großen Weichen im Detail |
| [AGENTS.md](AGENTS.md) | Konventionen für jede Agent-Session: Invarianten, Struktur, Verifikation |
| [unity/Assets/Hintergewerk/](unity/Assets/Hintergewerk/README.md) | **C#-Kern, SDK-frei** (Runtime/Editor/Tests, asmdefs) + Selbsttest |
| [testdata/beispielhaus.json](testdata/beispielhaus.json) | Beispieldaten im Schema v1 (für Import-/Export-Tests) |

## Kernidee in drei Sätzen

Beim Öffnen einer Wand zeichnet der Monteur Leitungen und Träger **mit der Hand in die Szene** —
Snap-Punkte und Sicherheitskorridore gleichen das Hand-Zittern aus, unter 30 Sekunden pro Objekt.
Monate später erscheint das Overlay: **Warnkorridor statt Haarlinie**, mit Foto, Tiefe und Monteur.
Export als JSON + PDF-Grundriss macht daraus einen Beweis, den Bauherr und nächste Firma annehmen.

## Links

- Wettbewerb: https://start-developer-competition-26.devpost.com/
- Meta VR Developers: https://developers.meta.com/vr/
- Developer Forum (Start Lounge, Support): https://communityforums.atmeta.com/
