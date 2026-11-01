# ADR 0002 — Anker-lokale Geometrie

**Status:** akzeptiert (01.11.2026)

## Kontext
Spatial Anchors driften und re-lokalisieren. Würden Installationen in Weltkoordinaten gespeichert, läge das Gedächtnis nach jeder Anker-Korrektur stillschweigend falsch — der Produktkern versagt, ohne dass es auffällt.

## Entscheidung
Geometrie wird **anker-lokal** gespeichert (`points_lokal` in `TrackedObject`). Beim Laden gilt `Punkt_world = AnchorPose × Punkt_lokal`. Das Feld `anchor_confidence` dokumentiert die Verlässlichkeit des Ankers; verwaiste Anker machen das Objekt „nicht verortbar" (sichtbar in der Liste, nie gelöscht).

## Alternativen
- **Weltkoordinaten:** verworfen — stiller Positionsfehler bei Re-Lokalisierung.
- **Weltkoordinaten + periodische Re-Basis:** verworfen — Migrationskomplexität, gleiches Grundproblem.

## Konsequenzen
- Objekte erben Drift-Korrekturen ihres Ankers automatisch.
- Der Export muss Anker-Posen auflösen (MapRenderer nimmt Weltpunkte entgegen).
- Die Ehrlichkeitsregel gilt bis in die Persistenz: Ein nicht verortbares Objekt wird angezeigt, nicht versteckt.
