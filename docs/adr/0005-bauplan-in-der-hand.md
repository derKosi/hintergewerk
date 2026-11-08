# ADR 0005: Bauplan in der Hand + BIM-ready statt Scan-to-BIM

**Status:** akzeptiert · **Datum:** 08.11.2026 · **verwandt:** D38–D40, ADR-0003

## Kontext

Die Produktvision wurde geschärft: Hintergewerk soll nicht „nur" den Raum erfassen, sondern der
Monteur trägt die Leitungen **mithilfe eines Bauplans in der Hand** hinter die Wand ein — eine
Top-Down-Sicht des aktuellen Raums, die er wie einen echten Bauplan in der Hand hält. Das Ergebnis
soll in ein **BIM** (Building Information Model) fließen, damit die Bauplanung damit arbeiten kann.

Die Frage: Wie bekommen wir beides ehrlich in die verbleibenden 10 Tage bis zur Abgabe (18.11.)?

## Entscheidung

**1. Bauplan = Output und Anzeige, nicht Input.** Der Bauplan wird **nicht gescannt oder geladen** —
er entsteht aus den eigenen Daten: Der `MapRenderer` projiziert das HouseModel bereits top-down
(PDF-Pfad, D15). Diese Projektion wird **live** als Textur auf einem Hand-Panel gerendert. Der Plan
wächst sichtbar mit, während der Monteur zeichnet. Kein neuer Export-Pfad, keine neue Geometrie —
Wiederverwendung. Kern-M4 (bis 13.11., Feature-Freeze).

**2. BIM-Anschluss ehrlich als „BIM-ready", nicht als „BIM".** Kein IFC/Revit-Export im
Wettbewerbsscope — in 10 Tagen nicht baubar und nicht verifizierbar. Stattdessen: Das schema-
versionierte HouseModel-JSON (Typ · Tiefe · Konfidenz · Monteur · Zeit, anker-lokale Geometrie) ist
der **Anschlussvertrag** für BIM-/Planungs-Workflows; der PDF-Grundriss ist die menschenlesbare
Ebene derselben Daten. Pitch-Sprache: „BIM-ready Export", nie „wir machen BIM" (Ehrlichkeitsregel).

**3. Bauplan als Zeichenebene ist Stretch.** Auf dem Panel tippen → Ray auf die Wandebene → Snap
(SnapEngine wiederverwendet) ist technisch machbar, aber die Winkelauflösung auf kleinem Panel ist
ein Risiko. Scope-Leiter-Sprosse 4 (docs/01 §5): Wenn es hakt, bleibt der Bauplan Live-Ansicht und
das Zeichnen bleibt an der Wand.

## Alternativen

- **Echtes IFC/Revit-Export:** unrealistisch bis 18.11., nicht am Gerät verifizierbar, verwässert den
  Kernflow. Verworfen.
- **Bauplan weglassen** (nur Export-PDF als Plan): spart Arbeit, verliert aber das Kopf-Feature und
  den Video-Moment („der Plan wächst in deiner Hand"). Verworfen.
- **Bauplan als Zeichenebene sofort als Kern:** verletzt das Feature-Freeze-Risiko — eine neue
  Interaktionsfläche 10 Tage vor Abgabe, ohne Gerätetest-Puffer. Verschoben auf Sprosse 4.

## Konsequenzen

- M4 v2: „Politur + Bauplan in der Hand" (docs/01 §3.2) — Acceptanz: Panel zeigt den Raum live,
  Objekt erscheint beim Confirm.
- Video-Storyboard (docs/08) bekommt den Beat „Bauplan wächst in der Hand" zwischen Capture und Export.
- Submission-Text (docs/09) formuliert „BIM-ready Export (JSON + PDF)" — kein BIM-Claim.
- Scope-Leiter v2 fixiert die Streich-Reihenfolge inkl. der neuen Sprosse (docs/01 §5).
- Keine Änderung am Datenmodell — Schema v1 bleibt der Anschlussvertrag (keine schema_version-Hochzählung).
