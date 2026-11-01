# ADR 0001 — Unity statt WebXR

**Status:** akzeptiert (31.10.2026, D4)

## Kontext
Der Produktkern hängt an persistenter räumlicher Verortung: Das Overlay muss Wochen später deckungsgleich liegen. WebXR (Immersive Web SDK) wäre agentenfreundlicher (Text-Loop, Hot Reload), hat aber die schwächste Anker-Persistenz und die schlechteste Handqualität im Quest-Browser — genau an der riskantesten Annahme des Produkts.

## Entscheidung
Unity 6 LTS + Meta XR All-in-One SDK, direkt und ohne WebXR-Detour. Der riskanteste Teil (Anker-Persistenz, Meilenstein M1) wird am Tag 1–2 auf dem Gerät validiert — vor jedem UI-Feature.

## Alternativen
- **WebXR / IWSDK:** verworfen — Anker-Persistenz reicht für das Produktversprechen nicht.
- **Unreal 5.8:** verworfen — der geschriebene C#-Kern wäre verloren, der SDK-Pfad ist Unity-first, 17 Tage Restzeit vertragen keinen Engine-Wechsel.

## Konsequenzen
- Editor-Arbeit läuft über Unity MCP (agentensteuerbar), Builds über Batch-Skript (`build.cmd`).
- Installationslast am Tag 1 höher (SDK, Android-Module) — abgefedert durch Setup-Guide mit Zeitbox und Abbruchkriterien.
