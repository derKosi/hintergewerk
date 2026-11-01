# Interaktion — Hands-first als Grammatik

Der komplette Kernflow ohne Controller, ohne Tastatur. Die Grammatik wird **vor** dem Bauen festgelegt und durchgehalten — Abweichungen sind Bugs.

## 1. Grammatik (5 Gesten)

| Geste | Aktion | Feedback |
|---|---|---|
| **Pinch** (Daumen + Zeigefinger) | zeichnen / greifen / bestätigen | Linie folgt der Hand, Snap-Klick bei Einrasten |
| **Zwei-Hand-Distanz** | Tiefe setzen / messen / skalieren | cm-Wert schwebt zwischen den Händen |
| **Handfläche zum Körper** | Menü auf/zu | Menü dockt an der Innenseite der Hand |
| **Wischen** (offene Hand) | Undo | Letztes Element pulsiert auf und verschwindet |
| **Blick + Pinch** | Objekt auswählen | Highlight + Detail-Panel (wer, wann, Tiefe, Foto) |

## 2. Präzisions-Realität

Hand-Tracking liegt bei 1–2 cm mit Zittern. Freihändig „exakt" zeichnen ist nicht das Ziel — die Grammatik macht Genauigkeit überflüssig:

1. **Wand-Snap**: Raycast auf Wand-/Deckenebene — Punkte kleben an der Fläche
2. **Raster**: 5-cm-Einrasten in der Fläche
3. **Magnetische Endpunkte**: Nähe zu bestehenden Objekten/Dosen zieht den Punkt an
4. **Zwei-Punkt-Bestätigung**: Start/Ende einzeln bestätigen statt freihändig fahren
5. **Sicherheitskorridor (± 5 cm)**: dargestellt als leuchtendes Band/Volumen, nie als Haarlinie

## 3. Capture-Flow (Ziel: < 30 s pro Objekt)

| Schritt | Aktion | Zeit |
|---|---|---|
| 1 | Handflächen-Menü → Typ radial wählen | 3 s |
| 2 | Pinch am Startpunkt (snappt) | 2 s |
| 3 | Linie entlangziehen, Punkte setzen | 10 s |
| 4 | Zwei-Hand-Distanz → Tiefe (Preset pro Typ) | 5 s |
| 5 | Foto anhängen (optional) | 5 s |
| 6 | Auto-Save + Bestätigungs-Toast | 2 s |

Konfidenz-Regel: Standard ist `geschätzt`; `gemessen` setzt der Monteur nur explizit (z. B. Tiefe war offen sichtbar).

## 4. Recall-Flow

1. App-Start → Overlay erscheint (Cold Start < 15 s)
2. Wand ansehen → Korridore leuchten im Blickfeld
3. **FoV-aware**: Objekt außerhalb des Blickfelds → Rand-Glow in der Richtung + optional räumlicher Audio-Ping bei Annäherung
4. Hand nähert sich Korridor → Warnung eskaliert (Farbe/Intensität)
5. Blick + Pinch → Detail-Panel mit Foto, Tiefe, Konfidenz, Monteur, Datum

## 5. Fehler & Undo

- Wischen = Undo (letzter Punkt / letztes Objekt)
- Destruktive Aktionen (Objekt löschen) nur im Detail-Panel mit Pinch-Bestätigung
- Verlorener Anker → Objekt in Liste als „nicht verortbar", nicht wegzaubern

## 6. Testkriterien (hard)

| Test | Kriterium |
|---|---|
| **Fremden-Test** | Person ohne Projektkenntnis schafft den Capture-Flow ohne Erklärung |
| **30-Sekunden-Test** | Vollständiges Objekt (Typ, Tiefe, Foto) in < 30 s |
| **Flugzeug-Sitz-Test** | Recall vom Hocker vollständig bedienbar (Design-Guideline!) |
| **Bus-Stop-Test** | Ein vollständiger Moment (Objekt capturen ODER Wand prüfen) in ≤ 2 Min |
| **Cold-Start-Test** | App-Start bis Overlay bereit < 15 s |
| **Neustart-Test** | Kill + Neustart: Overlay deckungsgleich (M1/M3) |
