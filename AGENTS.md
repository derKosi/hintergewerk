# AGENTS.md — Konventionen für Agent-Sessions in diesem Repo

Der Einstieg für jede (neue) Session — Mensch oder Agent. Vor der ersten Änderung lesen.

## Was dieses Projekt ist

**Hintergewerk** — ein VR-Werkzeug (Meta Quest), das verdeckte Installationen dokumentiert, solange die Wand offen ist, und Monate später als Sicherheitskorridor zurückbringt. Eingereicht bei der Meta VR Start Developer Competition 2026. **Abgabe: 18.11.2026, 21:00 CET.** Track Productivity, Division New Experience.

## Harte Invarianten (nie verletzen)

1. **Ehrlichkeitsregel:** Wir behaupten nicht, durch Wände zu sehen. Jedes Objekt trägt sichtbare Konfidenz (`measured | estimated | scanner`). „Nicht gefunden" heißt nie „nicht vorhanden". Verwaiste Objekte (Anker weg) werden als „nicht verortbar" geführt, nie gelöscht.
2. **Anker-lokale Geometrie:** Punkte werden relativ zum Anker gespeichert (`Punkt_world = AnchorPose × Punkt_lokal`). Nie world-space persistieren.
3. **Kein Meta-SDK im Runtime-Kern:** Alles unter `Runtime/` kennt kein `OVR*`. SDK-Zugriff ausschließlich über die Interfaces in `Runtime/Core/Interfaces.cs`; Adapter-Implementierungen liegen in `Assets/Hintergewerk/Adapters/` (außerhalb der Runtime-asmdef → Assembly-CSharp, dort ist OVR* erlaubt).
4. **schema_version:** Datenmodell-Änderung → `Schema.Version` erhöhen + Migration. Der Parser überspringt unbekannte Keys (Forward-Kompatibilität).
5. **Auto-Save:** Jede Mutation persisted sofort — es gibt keinen Save-Button.
6. **Keine Secrets** (Keystores, Tokens) im Repo. Keine erfundenen Screenshots, Testimonials oder Messwerte. Keine echten Gebäudedaten in der Demo (Testraum mit Fantasieadresse).

## Struktur

- `unity/Assets/Hintergewerk/Runtime/` — SDK-freier Kern: `Data/`, `Capture/`, `Recall/`, `Export/`, `Core/` (Interfaces) — asmdef `Hintergewerk.Runtime`
- `unity/Assets/Hintergewerk/Editor/` — Selbsttest-Menü + Batch-Build — asmdef `Hintergewerk.Editor`
- `unity/Assets/Hintergewerk/Tests/EditMode/` — NUnit-Tests — asmdef `Hintergewerk.Tests`
- `docs/01–12` — Planung, Architektur, Interaktion, Submission, Entscheidungslog, Setup, Smoketest, Video, Text, Windows-Buildserver, Szenen-Spec, Devpost-Felder
- `docs/adr/` — die großen Weichen im Detail (0001–0004)
- `testdata/` — Beispieldaten im Schema v1

## Verifizieren (ohne Gerät)

1. Unity öffnen → Menü **Hintergewerk → Kern-Selbsttest (JSON-Roundtrip)** → muss „bestanden" melden
2. Menü **Hintergewerk → PDF-Beispiel erzeugen** → PDF ansehen (Legende, Maßstab, Korridor)
3. Window → General → **Test Runner** → EditMode → alle Tests grün

Erst danach SDK-Adapter bauen (`Runtime/Adapters/`). Der Simulator reicht für Iteration; das Gerät ist die letzte Wahrheit.

## Arbeitsspielweise

- **Entscheidungen:** kurz im `docs/05-entscheidungslog.md` ergänzen (aufsteigend, D-Nummer weiterzählen). Große Weichen zusätzlich als ADR in `docs/adr/`.
- **Commits:** deutsch, Imperativ, ein Thema pro Commit.
- **HUMAN-Punkte** (docs/01 §6) respektieren: Start-Membership, Gerät/Dashboard, Video, Submission sind Menschenarbeit.
- **Neue Session?** Lies: README → docs/05 (Entscheidungen) → docs/02 (Architektur) → docs/01 (Plan/Status). Dann Status-Tabelle im README aktualisieren und loslegen.
