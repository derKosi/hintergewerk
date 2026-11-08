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

Vorbereitung: Die Adapter liegen bereits als Skeleton vor (`Adapters/OvrAnchorProvider` — prov→UUID-Map in „anker_map.json", LoadAll lädt per UUID). Ich baue via MCP eine zweite Minimal-Szene:
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

---

## M2/M3-Gerätetest (ab 07.11. gebaut, Agent-seitig im Simulator verifiziert)

App enthält Szenenliste: **02_AnkerTest → 03_CaptureAutoTest → 04_Recall → 01_Smoketest**.
Szenenwechsel am Gerät: **linke Handfläche zum Körper halten (≈½ s) + linker Pinch** → nächste Szene (kein adb nötig).

### E. M3 — Recall + Export am Gerät (~10 min)

1. Vorher in Szene 03 mindestens 1–2 Objekt zeichnen (2-Tap-Linie → Tiefe → Pinch bestätigen).
2. Szenenwechsel → **04_Recall**. Erwartung:
   - Status: „Geladen: N Objekte" (+ ggf. „· X nicht verortbar" — ehrlich, nicht weggefiltert)
   - Korridore als leuchtende Bänder in Typfarben auf der Wand
   - Hand an die Wand halten: Nähe → Status „Vorsicht", Bernstein-Puls; im Korridor → „STOPP", Signalrot + schneller Audio-Ping
3. **Export**: rechte Hand frei, **linker Pinch** (ohne Handfläche) → „Export fertig: …pdf + …json"
4. Abholen (Laptop, USB):
   ```
   adb pull /sdcard/Android/data/de.bautoolz.hintergewerk/files/ hgw_export/
   ```
   → JSON prüfen (objekte inkl. `points_lokal` anker-lokal), PDF öffnen (Legende, Maßstab).

### F. M2 — Capture-Zeit messen (~5 min)

1. Szene 03, Stoppuhr: Handfläche-Meny → Typ → Linie ziehen (2+ Taps) → Tiefe → Pinch-Pinch.
2. Ziel < 30 s inklusive Foto-Versuch. Status „Foto nicht verfügbar" ist **erwartbar** (D8: ScreenCapture am Gerät unklar — siehe docs/05 D33); Alternativen stehen im Entscheidungslog.

### Pass-Kriterien M3 (Beweis)

- [ ] Korridor nach App-Kill + Neustart deckungsgleich (Anker-lokal-Geometrie, docs/05 D30)
- [ ] Warn-Eskalation sichtbar + hörbar
- [ ] JSON + PDF am Laptop, JSON enthält verwaiste Objekte (falls vorhanden)
- [ ] Screenrecording der 3 Beats (Korridor auf, Warnung, Export)

### G. Bauplan in der Hand (ab 08.11., D39/ADR-0005 — ~5 min, im selben Block)

Szene 04 startet mit Bauplan-Panel an der **linken Hand** (Papier-Quad, Statuszeile darunter).

- [ ] Panel folgt der linken Hand, Papier zeigt zum Kopf (lesbar, nicht gespiegelt)
- [ ] Statuszeile zählt ehrlich: „X Objekte · Y verortbar · Z nicht verortbar"
- [ ] Gezeichnete Objekte (aus Szene 03) erscheinen **live** im Plan (0,5 s Poll) — Typfarben, Geschätztes gedimmt (65 % Alpha)
- [ ] Beide Räume sichtbar (Simulator-Beweis: 4 Korridore über 2 Räume; am Gerät: echte Daten)
- [ ] Nicht verortbare Objekte fehlen im Plan, stehen aber in der Statuszeile (Ehrlichkeit)
- [ ] Screenrecording: „Plan wächst in der Hand" — **Video-Beat für docs/08**
- [ ] Cold Start < 15 s (M4-Kriterium, docs/01 §2) — Stoppuhr ab App-Icon bis Statuszeile
