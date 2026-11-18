# Das Gedächtnis der Wand

*Projekt-Tagebuch · 18. November 2026 · Hintergewerk, eingereicht bei der Meta VR Start Developer Competition*

## Die goldene Stunde

Jede renovierte Wand sagt die Wahrheit genau einmal: solange sie offen ist. Dann sieht der Monteur das Wasserrohr hinter der Verkleidung, das Kabel unter dem Putz, den Träger, den niemand erwartet hätte. Dann wird zugemauert, gespachtelt, gestrichen — und die Wahrheit ist weg. Was bleibt, sind ein paar verschmierte Handyfotos im Bau-Chat und die Erinnerung des Monteurs, der vielleicht in drei Wochen Urlaub hat.

Wir alle kennen das Ende dieser Geschichte. Monate später bohrt jemand aufhängen und trifft die Leitung. Wasser in der Wand, Kurzschluss, Streit, wer es war. Nicht weil jemand dumm war, sondern weil das Gebäude vergessen hat.

Digitale Bauplanung löst das für Neubauten mit BIM-Modellen. Aber der Bestand — Millionen Altbauten — hat kein Modell. Er hat nicht mal eine Zeichnung. Er hat nichts.

Hintergewerk ist unser Versuch, dem Bestand ein Gedächtnis zu geben. Mit einer VR-Brille, mit den Händen, in unter dreißig Sekunden pro Objekt.

## Was wir gebaut haben

Der Monteur zieht die Brille auf, wenn die Wand offen ist. Er zeigt mit dem Zeigefinger auf die Wand, drückt Daumen und Zeigefinger zusammen — und zeichnet die Leitung dahin, wo sie wirklich liegt. Zwei Hände auseinander ziehen heißt: so tief liegt sie hinter der Oberfläche. Ein Foto beweist, wie es aussah. Nach dem Zusammendrücken ist das Objekt gespeichert: räumlich verankert, mit Typ (Strom, Wasser, Heizung, Träger), Tiefe, Zeitstempel und dem Namen, wer es erfasst hat.

Das Beste dabei: In der anderen Hand wächst der Bauplan. Ein echter Top-Down-Plan des Raums, live, in dem jeder Korridor erscheint, sobald der Monteur ihn bestätigt. Der Plan, den man sonst nachher am Schreibtisch aus Fotos bastelt, entsteht im selben Moment mit.

Monate später — beim nächsten Umbau, beim Bohren, bei der neuen Firma — kommt die Brille wieder auf. Das Overlay erscheint genau dort, wo die Leitung liegt. Nicht als Haarlinie, die man im Eifer verpasst, sondern als leuchtender Sicherheitskorridor von etwa fünf Zentimetern Breite. Kommt die Hand näher, eskaliert der Korridor: Farbe, Helligkeit, und ein Audio-Signal, das aus der Richtung der Gefahr kommt. Der Bohrer stoppt, bevor das Rohr leidet.

Eine Geste exportiert zum Schluss: PDF-Grundriss je Raum mit Legende und Maßstab — das Papier, das der Bauherr akzeptiert — plus eine maschinenlesbare JSON-Datei für Planungssoftware. Ehrlich strukturiert, aber kein proprietäres BIM-Format. Der Anschluss an die Bauplanung, nicht die Behauptung, wir hätten ein BIM-Modell gebaut.

## Die Regel, die alles trägt

Wir behaupten nicht, durch Wände zu sehen. Wir erinnern, was dokumentiert wurde.

Das klingt selbstverständlich, ist es aber nicht. Jede „X-Ray-App" verspricht, versteckte Leitungen zu finden. Wir fänden das unehrlich — und gefährlich: Wer einer App glaubt, sie sähe durch die Wand, bohrt, wo die App nichts angezeigt hat, und geht davon aus, da sei nichts. Bei uns heißt ein leerer Raum: „Hier wurde nichts dokumentiert." Nicht: „Hier ist nichts." Jedes Objekt trägt eine sichtbare Konfidenz — gemessen, geschätzt oder aus dem Raum-Scanner. Und wenn ein räumlicher Anker nach Monaten nicht mehr auffindbar ist, bleibt das Objekt als „nicht verortbar" in der Liste. Es wird nie stillschweigend gelöscht.

## Wie es technisch steht

Der Kern ist in C# geschrieben und absichtlich SDK-frei: Die Assembly `Hintergewerk.Runtime` kennt kein einziges `OVR*`-Symbol. Alles, was das Headset kann — Anker, Hände, Wände aus dem Room-Setup — fließt über schmale Interfaces, deren Adapter außerhalb des Kerns liegen. Dadurch läuft die gesamte Logik auch im Meta-XR-Simulator und in Unit-Tests: Zur Abgabe stehen 109 Tests auf grün, und der komplette Capture-Flow ist als durchgehender Beweislauf im Simulator aufgezeichnet.

Zwei Entscheidungen sind das Rückgrat:

**Geometrie ist anker-lokal gespeichert.** Ein Punkt liegt nie in Weltkoordinaten in der Datei, sondern relativ zu seinem räumlichen Anker: `Punkt_welt = AnkerPose × Punkt_lokal`. Wenn sich Anker verschieben oder die Brille den Raum neu lokalisiert, bleiben die gespeicherten Verläufe konsistent — die Korrektur passiert an einem Ort, nicht an hundert Punkten.

**Jede Änderung persistiert sofort.** Es gibt keinen Speichern-Knopf, weil es keinen Moment gibt, in dem man „fertig" klickt. Die Baustelle ist kein Formular.

Weil Hand-Tracking ein bis zwei Zentimeter zittert, ist Präzision kein Motorik-Thema, sondern ein System-Feature: Raycast-Snapping auf Wandebenen, ein Fünf-Zentimeter-Raster, magnetische Endpunkte an bestehenden Verläufen — und der Sicherheitskorridor, der dem Zittern ehrlich einen Spielraum gibt, statt eine Haarlinie vorzutäuschen.

## Was wir ehrlich nicht behaupten

Das Eingabevideo zeigt den Meta-XR-Simulator, gekennzeichnet ab Sekunde eins. Die Gerätetests (Anker über echten Neustart, echtes Raum-Setup) stehen auf unserer Liste — das Video verspricht sie nicht als erledigt. Auch bei der Persistenz bleiben wir bei dem, was wir zeigen können: gespeicherte Verläufe überleben komplette App-Neustarts; wie viele Tage und Neustarts es am Gerät wirklich sind, messen wir, bevor wir es behaupten.

## Was als Nächstes kommt

Der Raum-Scanner soll mitlesen: was er findet, wird in dasselbe Gedächtnis einsortiert. Ganze Häuser mit Treppen, Etagen, Gartenwegen. IFC-Export für Planungsbüros. Und ein Agent, der neue Trassen vorschlägt — auf Basis dessen, was das Haus schon weiß.

Wenn die Idee ein Satz bleibt, dann dieser: **Die Wand braucht nicht einfach zugesetzt zu werden. Sie muss sich erinnern können.**
