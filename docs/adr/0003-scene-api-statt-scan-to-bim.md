# ADR 0003 — Scene API statt Scan-to-BIM

**Status:** akzeptiert (31.10.2026, D15/D16)

## Kontext
Ursprungsidee: Räume selbst scannen (Möbel per Hand wegschieben), daraus Grundrisse/BIM erzeugen und Leitungen hineinzeichnen. Die Quest 3 hat kein LiDAR; ein eigener In-App-Scanner wäre das riskanteste Feature des Wettbewerbs — und Fotogrammetrie-Meshes liefern keine Semantik fürs Snappen.

## Entscheidung
Der „Scan" ist das **OS-seitige Room Setup** (Scene API: klassifizierte Ebenen + Scene Mesh + Möbel-Semantik). Der **Grundriss ist Output, nicht Input**: Der MapRenderer erzeugt ihn beim Export aus den erfassten Daten. Das Möbel-Freiräumen lebt im Geist-Modus (Stretch-Ziel, D16).

## Alternativen
- **LiDAR-Import (RoomPlan/Polycam):** auf die Roadmap verschoben — separates Gerät, Import-Pipeline, kein VR-Flow.
- **Scan-to-BIM (Matterport/ReCap/Revit):** Profi-Werkzeug, kein Hackathon-Scope.

## Konsequenzen
- Kein eigenes Scan-Modul zu bauen — die Zeit geht in Capture und Recall.
- Okklusion hinter Möbeln bleibt undokumentiert → die Konfidenz-Ehrlichkeit trägt das.
- Der Grundriss zeigt nur Erfasstes — kein Phantom-Gesamtmodell, konsistent mit der Ehrlichkeitsregel.
