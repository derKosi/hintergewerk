// Hintergewerk — Farbkanon für Korridore und Warn-Eskalation (docs/03 §2.5:
// „leuchtendes Band/Volumen, nie als Haarlinie" + „Warnung eskaliert Farbe/Intensität").
// Rein UnityEngine.Color, kein SDK — Korridor-Renderer und Vorschau-Linie nutzen dieselben Farben.
using UnityEngine;
using Hintergewerk.Data;

namespace Hintergewerk.Recall
{
    public static class RecallPalette
    {
        /// <summary>Typ-Farbe (Basis-Status: alles gut sichtbar, keine Warnung).</summary>
        public static Color TypeColor(ObjectType t) => t switch
        {
            ObjectType.Power => new Color(0.98f, 0.83f, 0.16f),  // Elektro: gelb
            ObjectType.Water => new Color(0.16f, 0.55f, 0.98f),  // Wasser: blau
            ObjectType.Heating => new Color(0.95f, 0.35f, 0.14f),// Heizung: rot-orange
            ObjectType.Stud => new Color(0.35f, 0.85f, 0.45f),   // Ständerwerk: grün
            ObjectType.NoDrillZone => new Color(0.85f, 0.25f, 0.85f), // Nicht-bohren: magenta
            _ => Color.white
        };

        /// <summary>Nähe-Stufe (Hand im Warnbereich): Bernstein-Puls auf der Basis-Farbe.</summary>
        public static Color NearColor(ObjectType t) => Color.Lerp(TypeColor(t), new Color(1.0f, 0.62f, 0.05f), 0.65f);

        /// <summary>Kritisch (Hand im Korridor): Signal-Rot, volle Deckung.</summary>
        public static Color CriticalColor(ObjectType t) => new Color(0.95f, 0.08f, 0.08f);

        /// <summary>Puls für Eskalations-Stufen (0..1), ruhelos aber nicht flackernd.</summary>
        public static float Pulse(float time, WarningLevel level) => level switch
        {
            WarningLevel.Near => 0.5f + 0.5f * Mathf.Sin(time * 4f),
            WarningLevel.Critical => 0.5f + 0.5f * Mathf.Sin(time * 9f),
            _ => 0f
        };
    }
}
