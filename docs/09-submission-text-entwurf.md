# Submission-Text — Entwurf v1 (Englisch, ~430 Wörter)

> Status: ENTWURF vom 01.11. — nach Feature-Freeze (10.11.) aktualisieren:
> Features streichen, die nicht gebaut wurden; „X days / Y restarts" mit echten Werten füllen.
> Wortlimit 500. Struktur folicht 04-submission.md §3.

---

## Hintergewerk — The Work Behind the Surface

**Inspiration**

Every renovated wall tells the truth exactly once: while it's open. Then we close it, and the
knowledge is gone — living in a few blurry photos, or in nothing at all. Months later, a drill
meets a water pipe nobody remembered. Building documentation either means scrolling through
photo chaos or a BIM model that presumes your house was planned digitally. The existing stock
of buildings has no memory. So we gave it one.

**What it does**

Hintergewerk — German for "the work behind" — captures what an open wall reveals, directly in VR, with your hands. Pinch to
draw a cable run along the open wall. Two hands set the depth. A photo proves it. Under thirty
seconds per object, the moment becomes permanent: spatially anchored, typed (power, water,
heating, studs), timestamped, and signed with the installer's name. Weeks later, the overlay
returns exactly where you left it — not as a thin line, but as a glowing safety corridor. When
your hand approaches, the corridor escalates: color, intensity, spatial audio. The drill stops
before the pipe does. Look and pinch, and the original evidence appears: the photo, the depth,
who documented it — and whether it was measured or estimated. One gesture exports a floor plan
with legend and scale as PDF and a machine-readable JSON: the document a homeowner accepts and
the next firm inherits.

**How we built it**

Unity 6 with the Meta XR SDKs. The core is deliberately engine-agnostic: a versioned data
schema where geometry is stored anchor-relative, so anchor drift and re-localization can never
corrupt a captured run. Snap points, a 5 cm grid and magnetic endpoints absorb hand-tracking
tremor; a ±5–6 cm safety corridor replaces the illusion of millimeter lines. Spatial anchors
persist across app restarts and re-entry (demonstrated, not claimed: X days, Y restarts).
The PDF export is a dependency-free emitter written from scratch — lines, legend, scale bar.

**Hand interactions — and why we love the future of VR**

Hands are the honest input for this job: an installer holds nothing but eyes and fingers.
Our grammar is five gestures — pinch to draw and confirm, two-hand distance to measure depth,
palm-to-body for the menu, swipe to undo, gaze-plus-pinch to inspect. Nothing else. Hands-first
forced us to design precision as a system property instead of a motor skill — and that made the
product better.

**Honesty (our design rule)**

We do not see through walls. We remember what was documented. Every object carries a visible
confidence level, and „not found" never means „not there."

**What's next**

Scanner readings joining the memory, whole houses with staircases, LiDAR imports, IFC export
for planning firms — and agents that propose new routing around what the house already knows.

---

*Tagline (140 Zeichen, Auswahl D13 am 13.11.):*
„Hintergewerk — the work behind the surface. Capture what an open wall reveals with your hands,
in 30 seconds. Find it again months later, to the centimeter."
