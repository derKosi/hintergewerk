# Simulator-Demo-Video — Skript (16.11.2026, Agent)

**Zweck:** Vorab-Montage der Abgabe-Beats aus Simulator-Material (docs/08 „Ein-Rutsch-Drehplan").
Die finalen Clips am Gerät ersetzen die Simulator-Bilder 1:1 (gleiche Beats, gleiche Sprechtexte).
Alle Texte EN (D20: Video EN), darunter die DE-Übersetzung für Review.

**Ehrlichkeits-Hinweis im Film (Beat 1, klein):** „Meta XR Simulator — device footage to follow".
Keine erfundenen Messwerte, keine Geräteszenen, die nie so liefen.

## Beats (Gesamt ~70 s)

| # | Zeit | Bild (Quelle) | VO EN | VO DE |
|---|---|---|---|---|
| 1 | 0:00–0:06 | Titelkarte (Endkarte dunkel + Text-Overlay) | „Hintergewerk. The work behind the surface. This is the Meta XR Simulator — device footage to follow." | „Hintergewerk. Das Wissen hinter der Oberfläche. Zu sehen: der Meta XR Simulator — Gerätebilder folgen." |
| 2 | 0:06–0:20 | live_capture2.mp4 0–14 s: erster Drag, gelbe Linie wächst | „The wall is open. The worker draws an electrical line in mid-air — pinch, pull, and the grid snaps every point." | „Die Wand ist offen. Der Monteur zeichnet eine Elektroleitung in die Luft — Pinch, Zug, und das Raster fängt jeden Punkt." |
| 3 | 0:20–0:32 | live_capture2.mp4 14–26 s: Menü-Geste, blaue Wasserleitung | „A pinch cycles the type: water. Each type has its own colour — and its own safety corridor." | „Ein Pinch wählt den Typ: Wasser. Jeder Typ hat seine Farbe — und seinen eigenen Sicherheitskorridor." |
| 4 | 0:32–0:40 | Still: Bauplan mit 3 Objekten (`f1_s03_end.png`) | „The blueprint in the other hand grows with every line. Three lines, three types — documented in seconds." | „Der Bauplan in der anderen Hand wächst mit jeder Linie. Drei Linien, drei Typen — dokumentiert in Sekunden." |
| 5 | 0:40–0:52 | live_korridore-Bild/Clip (`f6_s04_korridore_frontend.png`) | „Months later, the app remembers. Safety corridors replace hairlines — yellow for electric, blue for water, red-orange for heating." | „Monate später erinnert sich die App. Sicherheitskorridore statt Haarlinien — Gelb für Elektro, Blau für Wasser, Rot-Orange für Heizung." |
| 6 | 0:52–1:00 | Still: Export-Dateien (Dateinamen-Overlay) | „One gesture exports a PDF floor plan and BIM-ready JSON — the handover the next crew can actually use." | „Eine Geste exportiert PDF-Grundriss und BIM-ready-JSON — die Übergabe, mit der die nächste Kolonne wirklich arbeiten kann." |
| 7 | 1:00–1:10 | Endkarte (`endcard_1920x1080.png`) | „Nothing invented. Everything measured — or honestly marked. Hintergewerk." | „Nichts erfunden. Alles gemessen — oder ehrlich markiert. Hintergewerk." |

## Assets

- `live_capture2.mp4` — kompletter Capture-Flow (Run 5, Szene 03, Möbel-Raum-Umgebung)
- `live_korridore.mp4` — Szene 04, drei Korridore in Typfarben (mit Bauplan-Panel)
- `f1_s03_end.png` — Bauplan „Geladen: 3 Objekte"
- `f6_s04_korridore_frontend.png` — 3 Korridore gelb/blau/rot-orange
- Export heute: `hintergewerk_raum_1_20261116_1540.pdf` + `hintergewerk_export_20261116_1540.json`
- Endkarte/What's-next aus diesem Ordner (10.11.)

## TTS

Stimmen: `en-US-AndrewNeural` (EN, warm/confident) · `de-DE-KatjaNeural` (DE-Review-Version).
Je Beat eine MP3 → Montage richtet die Beat-Länge nach der Sprechdauer (ffmpeg concat + apad).
