# Abgabe final — zum Einfügen (18.11.2026)

Alle Felder fertig. Stand: Feature-Freeze real (16.11. Simulator-Beweis, 109/109 Tests grün).
Einzige Änderung ggü. Entwurf v4: Persistenz-Satz ehrlich ohne erfundene Zahlen (Regel docs/01 §6, docs/12 Checkliste).

## Formularfelder

| Feld | Wert |
|---|---|
| **Submission name** | Hintergewerk |
| **Tagline** (≤ 140 Zeichen, 74) | Hintergewerk — the work behind the surface. Trace it once, find it forever. |
| **Track** | Productivity |
| **Division** | New Experience |
| **Video-URL** | ⟨YouTube — Datei: `docs/video-assets/hintergewerk_sim_demo_de.mp4` (DE) / `..._en.mp4` (EN), 74 s, H.264 yuv420p, verifiziert⟩ |
| **Build/Invite-URL** | ⟨APK-Download-Link, z. B. auf dem bau-toolz-Server neben der Website; APK nach Rebuild⟩ |
| **Team** | Mathias — Concept, VR Development, Demo |
| **Target launch date** | Q1 2027 (App Lab, nach Jury-Feedback) |

## Beschreibung (EN, ~475 Wörter — einfügen als ein Block)

Hintergewerk — The Work Behind the Surface

**Inspiration**

Every renovated wall tells the truth exactly once: while it's open. Then we close it, and the knowledge is gone — living in a few blurry photos, or in nothing at all. Months later, a drill meets a water pipe nobody remembered. Existing documentation means scrolling through photo chaos, or a BIM model that presumes your house was planned digitally. The existing stock of buildings has no memory. So we gave it one.

**What it does**

Hintergewerk — German for "the work behind" — captures what an open wall reveals, directly in VR, with your hands. Pinch to draw a cable run along the open wall. Two hands set the depth. A photo proves it. Under thirty seconds per object, the moment becomes permanent: spatially anchored, typed (power, water, heating, studs), timestamped, and signed with the installer's name. While you draw, a floor plan grows in your other hand — the actual room, every corridor appearing as you confirm it. Weeks later, the overlay returns exactly where you left it — not as a thin line, but as a glowing safety corridor. When your hand approaches, the corridor escalates: color, intensity, and a panned audio ping that comes from the direction of the hazard. The drill stops before the pipe does. Look and pinch, and the original evidence appears: the photo, the depth, who documented it — and whether it was measured or estimated. One gesture exports a floor plan per room with legend and scale as PDF, plus a machine-readable, BIM-ready JSON: the document a homeowner accepts, the next firm inherits, and planning software can consume.

**How we built it**

Unity 6 with the Meta XR SDKs. The core is deliberately SDK-agnostic: a versioned data schema where geometry is stored anchor-relative, so anchor drift and re-localization can never corrupt a captured run. Snap points, a 5 cm grid and magnetic endpoints absorb hand-tracking tremor; a ±5–6 cm safety corridor replaces the illusion of millimeter lines. The in-hand floor plan is the same top-down projection that renders the exported PDF — one source of truth, live instead of on paper — and both draw the real room outline from the Quest Scene API. Whole houses work: rooms sort themselves — each captured run joins the room whose wall it touched, every room gets its own plan, one JSON holds them all. Persistence is demonstrated where we can demonstrate it: captured runs survive full app restarts, stored anchor-relative in device storage — and we don't claim multi-day device numbers until we've measured them. The PDF export is a dependency-free emitter written from scratch: lines, legend, scale bar.

**Hand interactions — and why we love the future of VR**

Hands are the honest input for this job: an installer holds nothing but eyes and fingers. Our grammar is five gestures — pinch to draw and confirm, two-hand distance to measure depth, palm-to-body for the menu, swipe to undo, gaze-plus-pinch to inspect. Nothing else. Hands-first forced us to design precision as a system property instead of a motor skill — and that made the product better.

**Honesty (our design rule)**

We do not see through walls. We remember what was documented. Every object carries a visible confidence level — measured, estimated or scanner — and "not found" never means "not there." Objects whose anchor can no longer be located stay in the record as "not locatable," never silently deleted. The export is honest about its format too: BIM-ready structured data — not a proprietary BIM file, and we never claim otherwise.

**What's next**

Scanner readings joining the memory, whole houses with staircases, LiDAR imports, IFC export for planning firms — and agents that propose new routing around what the house already knows.

## Hand Interactions (EN — einfügen als ein Block)

Hintergewerk is built hands-first end to end — no controllers, no keyboard. Five gestures cover the whole flow: pinch to draw and confirm, two-hand distance to set installation depth, palm-to-body to open the menu, swipe to undo, gaze-plus-pinch to inspect. Because hand tracking lives at 1–2 cm of tremor, precision is a system property instead of a motor skill: raycast snapping to wall planes, a 5 cm grid, magnetic endpoints on existing runs, and a ±5 cm safety corridor instead of a hairline. Every surface is a plane — walls today, floors, ceilings and garden routes later — so the same grammar works anywhere the trades work.

## Screenshots (Reihenfolge, docs/12)

1. Capture: offene Wand, gezeichnete Leitung mit Korridor
2. Tiefe-Geste: cm-Wert zwischen den Händen
3. Recall: Overlay + eskalierende Warnung (gelb → rot)
4. Detail-Panel: Foto, Tiefe, Konfidenz, Monteur, Datum
5. Export: PDF-Grundriss mit Legende, Maßstab und Raum-Umriss (D41)
6. Bauplan in der Hand: Live-Top-Down-Plan mit echtem Zimmer-Umriss + Statuszeile (D39/D41)
7. Zwei Räume, automatisch sortiert (D43)

## Rest-Arbeit (Menschenarbeit, docs/01 §6)

- [ ] Rebuild abwarten → APK auf bau-toolz-Server hochladen → Link in Build/Invite-URL
- [ ] Video auf YouTube öffentlich (≤ 3:00 ✓ 74 s) → URL einfügen
- [ ] Screenshots 1–7 vom Gerät/Sim einfügen
- [ ] Invite-Link frisch testen (anderes Konto)
- [ ] Absenden + Bestätigungs-E-Mail sichern
