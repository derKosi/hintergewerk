# The Memory of a Wall

*Project diary · November 18, 2026 · Hintergewerk, submitted to the Meta VR Start Developer Competition*

## The golden hour

Every renovated wall tells the truth exactly once: while it's open. The plumber sees the pipe behind the paneling, the cable under the plaster, the load-bearing stud nobody expected. Then it gets boarded up, skimmed, painted — and the truth is gone. What remains are a few blurry phone photos in the construction chat and the memory of an installer who may be on vacation in three weeks.

We all know how that story ends. Months later, someone drills to hang a shelf and hits the pipe. Water inside the wall, a short circuit, an argument about who is to blame. Not because anyone was careless, but because the building forgot.

Digital planning solves this for new builds with BIM models. The existing stock — millions of older buildings — has no model. It doesn't even have a drawing. It has nothing.

Hintergewerk is our attempt to give the existing stock a memory. With a VR headset, with your hands, in under thirty seconds per object.

## What we built

The installer puts on the headset while the wall is open. He points at the wall and pinches — and draws the run right where it actually lies. Spreading two hands apart sets how deep behind the surface it sits. A photo proves what it looked like. On confirm, the object is stored: spatially anchored, with its type (power, water, heating, stud), depth, timestamp, and the name of the person who captured it.

The best part: in the other hand, the floor plan grows. A real top-down plan of the room, live, with every corridor appearing as soon as the installer confirms it. The plan that normally gets reconstructed at a desk from photos afterwards takes shape in the same moment.

Months later — during the next renovation, when drilling, when a new firm arrives — the headset goes back on. The overlay appears exactly where the run lies. Not as a hairline you miss in the rush, but as a glowing safety corridor roughly five centimeters wide. As a hand approaches, the corridor escalates: color, brightness, and an audio cue that comes from the direction of the hazard. The drill stops before the pipe does.

One gesture exports at the end: a PDF floor plan per room with legend and scale — the paper the homeowner accepts — plus a machine-readable JSON for planning software. Honestly structured, but not a proprietary BIM format. A connection to the planning world, not a claim that we built a BIM model.

## The rule that carries everything

We do not claim to see through walls. We remember what was documented.

That sounds obvious. It isn't. Every "X-ray app" promises to find hidden pipes. We consider that dishonest — and dangerous: if you trust an app to see through the wall, you drill where the app shows nothing and assume nothing is there. In our app, an empty room means "nothing was documented here." Not "nothing is here." Every object carries a visible confidence level — measured, estimated, or from the room scanner. And if a spatial anchor can no longer be found months later, the object stays in the list as "not locatable." It is never silently deleted.

## How the technical side stands

The core is written in C# and deliberately SDK-free: the `Hintergewerk.Runtime` assembly contains no `OVR*` symbol. Everything the headset can do — anchors, hands, walls from room setup — flows through narrow interfaces whose adapters live outside the core. As a result, the entire logic also runs in the Meta XR Simulator and in unit tests: at submission time, 109 tests are green, and the complete capture flow is recorded as one continuous proof run in the simulator.

Two decisions form the backbone:

**Geometry is stored anchor-relative.** A point is never persisted in world coordinates, but relative to its spatial anchor: `point_world = anchorPose × point_local`. When anchors drift or the headset re-localizes the room, the stored runs stay consistent — the correction happens in one place, not at a hundred points.

**Every change persists immediately.** There is no save button, because there is no moment where you click "done." A construction site is not a form.

Because hand tracking trembles by one to two centimeters, precision is not a motor skill here but a system property: raycast snapping onto wall planes, a five-centimeter grid, magnetic endpoints on existing runs — and the safety corridor, which honestly gives the tremble a margin instead of pretending to draw a hairline.

## What we honestly do not claim

The submission video shows the Meta XR Simulator, labeled as such from second one. The on-device tests (anchors across a real restart, real room setup) are on our list — the video does not claim they are done. On persistence we stick to what we can show: stored runs survive full app restarts; how many days and restarts that amounts to on the actual device, we will measure before we claim it.

## What comes next

The room scanner should join in: what it finds gets filed into the same memory. Whole houses with staircases, floors, garden routes. IFC export for planning firms. And an agent that proposes new routing — based on what the house already knows.

If the idea has to fit in one sentence, it is this one: **A wall shouldn't just get closed up again. It needs the ability to remember.**
