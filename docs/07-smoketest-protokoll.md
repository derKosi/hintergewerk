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

Konkrete Klickfolge (09.11. geschärft — Flow lief NIE, M0 ging via adb; erstmals ZUSAMMEN mit D testen):
- [ ] Meta Developer Dashboard → Backend dieser App („Hintergewerk", `de.bautoolz.hintergewerk`) → **Release Channels / Distribution** (ggf. „Alpha")
- [ ] Channel „Competition": **Upload** der APK (aktuell: `unity/Builds/hintergewerk.apk`, 09.11. 14:16) — Upload dauert 1–2 min, dann verarbeitet Meta asynchron (Status prüfen, bis „ready")
- [ ] **Invite-/Share-Link** für den Channel erzeugen und kopieren
- [ ] Link aufs Handy schicken / QR zeigen — die Quest muss mit dem **gleichen Meta-Account** verknüpft sein wie die App-Team-Rolle

### D. Install über den Jurys-Weg (~10 min) — NICHT sideloaden!

- [ ] Quest-Browser → Invite-URL öffnen → Install-Flow durchlaufen (ggf. Login/„App anfordern")
- [ ] App starten: Würfel sichtbar? Hände getrackt (Skeleton sichtbar)?
- [ ] **Beweis:** Screenrecording oder Foto + Datum
- [ ] Stolperfalle: „App nicht verfügbar" → meist Channel-Mitgliedschaft/Team-Rolle fehlt oder Build noch in Verarbeitung (10 min warten, erneut)

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

**Zimmer-Umriss (D41/D42, ab 09.11.) — logcat nach `[SceneWallProvider]` filtern:**
- [ ] **Mit Room Setup** (vorher einmalig machen — brauchen wir eh für Capture-Snap): Log meldet
  `LoadSceneModel(): angefordert` + danach `N Wände via scene` (echte Wände) → Plan zeigt den
  **echten Raum-Umriss** (dunkle Grundkanten)
- [ ] **Ohne Room Setup**: Log meldet `via MockRaume_ohneRoomSetup` → Fantasy-Raum 3×3×2,6 m im
  Plan (Demo-Fallback, bewusst sichtbar geloggt — kein Fake „stille Wände")
- [ ] Export-Pinch: Log `Export] n Objekte, k Wand-Segmente, …` — k > 0 bei echten Wänden;
  PDF zeigt Umriss-Linien + Legende
- [ ] Szene 03 (Capture): zeichnet jetzt auf die **echten Wände** (Snap = Plan-Wände, D36-Pfad);
  Log `[Bauplan] DrawController auf Wall-Provider gesetzt` ist Editor-Spur, am Gerät egal

**Im XR-Simulator (D42):** Sim-Demo-Räume (Bedroom/GameRoom/LivingRoom) liefern KEINE
Scene-Anker — nur das Bild. Für den vollen Sim-Beweis: **Meta-XR-Simulator-Fenster → „Scene"-Tab
→ Room with Furnitures laden** (Menschen-Klick), dann Log `via scene` mit den 4 WALL_FACE-Wänden
(nicht-orthogonal! Grundriss muss mitgedreht erscheinen). Ohne Toggle: toter Platzhalter-Anker
(tracked=False) wird übersprungen → Mock greift — **das ist der erwartete ehrliche Zustand**.

### H. Automatische Raum-Zuordnung (D43, ab 10.11. — ~5 min, im selben Block wie §G)

Szene 03 (Capture) sortiert gezeichnete Objekte jetzt **selbst in den Raum der gesnappten Wand**
(Scene API: `OVRSceneRoom`-Zuordnung, Raum-Id = Room-Anchor-UUID; Mock: `raum_1`). Zweiter Raum
im OS-Room-Setup anlegen (oder vorhandenen nutzen) → um die Ecke gehen → dort zeichnen →
logcat prüfen:

- [ ] Log beim ersten Punkt je Raum: `[DrawController] Raum-Anker erstellt: <uuid> (Raum <roomId>)`
      — roomId JE Raum unterschiedlich (UUID), im Mock `raum_1`
- [ ] `[SceneWallProvider] … Räume: <uuid8>(<LABEL>)` — Label aus der Raum-Klassifikation
      (z. B. LIVING_ROOM); ohne Klassifikation: nur UUID, „keine erkannt" = Fallback aktiv
- [ ] `[DrawController] Objekt in Raum <roomId> gespeichert` — Objekt A (Raum 1) ≠ Objekt B (Raum 2)
- [ ] Szene 04: Statuszeile zählt beide Räume; Export-Pinch → **je Raum ein PDF**
      (`hintergewerk_<uuid>_….pdf`), JSON global mit beiden Räumen
- [ ] **Ehrlichkeits-Fall:** Wand OHNE Raum-Erkennung → Objekt landet in `raum_1`
      (Standard), Log sagt nicht mehr als es weiß — kein erfundener Raum

---

## M1-NO-GO-Fallback (10.11. entscheidungsreif geschrieben — NUR bei Bedarf aktivieren)

**Trigger:** M1 Run 1 **oder** Run 2 schlägt fehl (Würfel nicht deckungsgleich / Anker lädt
nicht). Run 3 allein ist KEIN Fail (Raumwechsel darf ≤ 5 cm versetzen). Da der Block erst
Do/WE läuft (docs/01 §3.3), steht der Fallback VOR dem Test fest — im Fail-Fall sofort
ausführbar, kein Entwurf mehr nötig.

**Was dann gebaut wird (~0,5–1 Tag Agent-Arbeit, vor Abgabe 18.11. machbar):**
1. `RecallController`-Fallback: Objekt ohne geladenen Anker-Pose wird nicht mehr nur
   „nicht verortbar" — es hängt sich an die **Scene-Wand** (SceneWallProvider): Objekt-Punkte
   werden auf die handnächste Wand projiziert und im **Raum-Bezug** angezeigt (Ersatz-Anker
   = Raumursprung aus dem Scene-Model). Korridor bleibt, Konfidenz fällt sichtbar auf
   **estimated** (Ehrlichkeitsregel: der Umweg wird angezeigt, nicht versteckt).
2. „Anker neu setzen"-Geste (Szene 04): Blick auf die Wand + Pinch = frischer Anker an dieser
   Stelle; Objekte im Raum werden gegen ihn neu verortet (prov→UUID-Map bleibt, UUIDs ändern
   sich nicht — nur die Pose-Qualität).
3. Tests: Fallback-Pfad unit-getestet (Pose fehlt → Wand-Ref her), Regression auf den
   Normal-Pfad; Story-Anpassung Video (Beat 0:40): „anchored — and if an anchor ever loses
   its grip, the room itself holds the memory until you re-pin it."

**Was NICHT ändert:** Datenmodell (anker-lokal bleibt — der Fallback ersetzt nur die
Pose-Quelle bei der Anzeige), Capture-Flow, Export. Schema bleibt v1.

**Story-Satz (ehrlich, für Video/Submission):** „Hintergewerk trusts anchors but verifies:
if an anchor cannot re-localize, the room's own walls carry the memory — visibly marked
estimated until re-pinned."
