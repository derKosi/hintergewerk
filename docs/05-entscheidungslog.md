# Entscheidungslog

Format: eine Entscheidung pro Zeile — **Was · Warum · Konsequenz.** Neue Entscheidungen unten anfügen (aufsteigend datiert), nichts löschen.

| Datum | ID | Entscheidung | Status | Kontext / Konsequenz |
|---|---|---|---|---|
| 31.10. | D1 | Track: **Productivity** | ✅ | Kreative Tools / Effektivität — passt zum Produktkern |
| 31.10. | D2 | Division: **New Experience** | ✅ | Projekt entsteht komplett im Fenster ab 24.09. — regelkonform |
| 31.10. | D3 | Start-Mitgliedschaft beantragt | ✅ | Bestätigungsmail erhalten — Gate 1 zu |
| 31.10. | D4 | Stack: **Unity 6 + Meta XR SDK** | ⏳ GO 01.11. | Anker-Persistenz ist die kritische Annahme → direkt nativ, kein WebXR-Umweg. GO nach erfolgreichem Setup |
| 02.11. | D5 | Smoketest Invite-URL-Flow | ⏳ | GO/NO-GO; bei NO-GO sofort Devpost-/Meta-Support |
| 02.11. | D6 | Anker-Persistenz | ⏳ | GO → Produkt wie geplant. NO-GO → Re-Lock-Fallback über Scene Mesh, Story anpassen |
| 03.11. | D7 | Repo (GitHub/Forgejo) + Lizenz | ⏳ | Privat reicht; keine OSS-Pflicht |
| 06.11. | D8 | Beweisfoto-Ansatz | ⏳ | Passthrough-API verifizieren; Fallback App-Render-Snapshot |
| 07.11. | D9 | 4. Objektklasse „Nicht-bohren-Zone" | ⏳ | Nur wenn 05.–07. glatt läuft |
| 10.11. | D10 | Agenten-Vorschlag bauen? | ⏳ | Nur bei stabilem Recall (Agentic Interaction Award) |
| 10.11. | D11 | Mitte-Scope-Check | ⏳ | Video-Fokus festlegen (Scope-Leiter in 01-projektplanung §5) |
| 10.11. | D12 | PDF vs. PNG-Export | ⏳ | Nach Aufwand des PdfEmitters |
| 13.11. | D13 | Finaler App-Name + Tagline | ⏳ | Arbeitstitel: „Hausgedächtnis" |
| 14.11. | D14 | Video-Framing | ⏳ | Sitzend-Szenen wo möglich, „X Tage / Y Neustarts"-Ehrlichkeit |
| 31.10. | D15 | Raumerfassung: **Scene API, Grundriss als Export** — kein Scan-to-BIM | ✅ | Blueprint ist Output, nicht Input · Details: 02-architektur.md §7 |
| 31.10. | D16 | **Geist-Modus + Möbel-Push** als Stretch nach M2; Onboarding über System-Room-Setup | ⏳ Gate 07.11. | Scan-Erlebnis = OS-Flow, kein eigenes Scan-Modul (kein LiDAR, keine Semantik) · Details: 02-architektur.md §7.1 |
| 01.11. | D17 | SDK-freier C#-Kern vorgezogen und fertig geschrieben; Meta-SDK nur noch in dünnen Adaptern hinter `Core/Interfaces.cs` | ✅ | Setup-Tag-Verlust aufgefangen · Kompilierung + Selbsttest am 02.11. · Code ist ungetestet bis dahin |
| 01.11. | D18 | Windows-3090-Kiste als **sekundärer Build-Runner** (Git + SSH + Batch-Build); **Unreal verworfen**; Mac bleibt Primärmaschine (AutoClaw + MCP) | ⏳ Setup 02.–03.11. | Anleitung: 10-windows-buildserver.md · Fernsteuerung (Parsec/RDP) optional für Mathias · M0/M1 hat Vorfahrt |
