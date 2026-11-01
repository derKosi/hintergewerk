# Smoketest-Protokoll — M0 (Pipeline) + M1 (Anker) — Di, 02.11.

**Zweck:** Die zwei unbekanntesten Dinge des Projekts zuerst entgiften.
**Regel:** Alles wird als Beweis gesichert (Fotos/Screenrecording vom Handy auf dem Headset-Display ist okay).
Menünamen im Dashboard können abweichen — das Ziel zählt, nicht der Pfad.

## M0 — Dev-to-Device-Pipeline (der 18.11.-Risikopunkt Nr. 1)

### A. Dashboard vorbereiten (~10 min)

- [ ] developers.meta.com → Manage Apps → **Create App**
      Name „Hintergewerk", Typ App/API — wie angeboten (Single-App reicht)
- [ ] In der App: **Builds / Release Channels** → neuen Channel **„Competition"** anlegen
      (genau dieser Name — die Wettbewerbs-richtlinie verlangt ihn)
- [ ] Tester hinzufügen: eigener Meta-Account (E-Mail der Quest)

### B. Build (~15 min)

- [ ] Ich baue die Minimal-Szene via MCP: Würfel 1 m vor Kamera-Start + Hand-Tracking-Skeleton-Visuals, sonst nichts
- [ ] File → Build Settings (Android) → Szene hinzu → **Build** → `hintergewerk-smoketest.apk`
- [ ] Build-Log abspeichern (Beweis)

### C. Upload + Invite-URL (~10 min)

- [ ] APK in den Channel „Competition" hochladen (ggf. „Alpha"-Flow)
- [ ] **Invite-/Share-Link** für den Channel erzeugen und kopieren
- [ ] Link aufs Handy schicken / QR zeigen

### D. Install über den Jurys-Weg (~10 min) — NICHT sideloaden!

- [ ] Quest-Browser → Invite-URL öffnen → Install-Flow durchlaufen
- [ ] App starten: Würfel sichtbar? Hände getrackt (Skeleton sichtbar)?
- [ ] **Beweis:** Screenrecording oder Foto + Datum

**M0-Pass =** Start über Invite-URL funktioniert mit Händen.
**M0-Fail ⇒** sofort Devpost/Meta-Support + Forum-Post (nicht basteln), parallel morgen wiederholen.

## M1 — Anker-Persistenz (die riskanteste Produkt-Annahme)

Vorbereitung: Ich baue via MCP eine zweite Minimal-Szene:
- Beim Pinch: Spatial Anchor an der Handposition erzeugen + roter Würfel 50 cm daneben (anker-lokal versetzt)
- Anchor speichern (`OVRSpatialAnchor.Save`), Log mit anchor-UUID
- Beim App-Start: Anker laden, Würfel platzieren; On-Screen-Text: „Anker geladen: <n>"

### Durchlauf (3×)

- [ ] **Run 1:** App starten → 1 Anker setzen → Wartetext „saved" abwarten → App beenden (Menü im Headset, nicht nur Headset aus)
- [ ] App neu starten → Anker geladen? → Würfel an derselben Stelle (± 2 cm)?
- [ ] **Run 2:** Wie Run 1, aber danach Headset 2× am Kopf absetzen/anders aufsetzen (Tracking-Kaltstart simulieren)
- [ ] **Run 3:** Wie Run 1, aber Raum kurz verlassen und wieder betreten (Guardian-Boundary-Neujustage erlauben)
- [ ] **Beweis:** je Run Foto + die Anchor-UUIDs aus dem Log

**M1-Pass =** Run 1+2 deckungsgleich, Run 3 maximal leicht versetzt (≤ 5 cm).
**M1-Fail ⇒** Entscheidung D6 (Fallback): Re-Lock über Scene-Mesh-Ebenen + manuellen Bestätigungs-Flow;
Produkt-Story „Hintergewerk mit Bestätigung statt blinder Anker-Gläubigkeit" — ehrlich, machbar, aber anders.

## Nach bestandenem M0+M1

- Status-Tabelle im README aktualisieren (M0 ✅ / M1 ✅ + Datum)
- Entscheidung D4 (Stack) endgültig auf GO setzen
- Ab 03.11.: SDK-Adapter an die Interfaces (`Core/Interfaces.cs`), dann Capture-Flow
