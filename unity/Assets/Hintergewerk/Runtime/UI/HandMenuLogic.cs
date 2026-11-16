// Hintergewerk — reine Menü-Logik (Grammatik §1: Handfläche = Menü, Pinch = blättern).
// Ohne Unity-UI: Die visuelle Radial-UI kommt später und fragt nur IsOpen/CurrentType ab.
using Hintergewerk.Data;

namespace Hintergewerk.UI
{
    public class HandMenuLogic
    {
        static readonly ObjectType[] Types =
            { ObjectType.Power, ObjectType.Water, ObjectType.Heating, ObjectType.Stud, ObjectType.NoDrillZone };

        public bool IsOpen { get; private set; }
        public int Index { get; private set; }
        bool palmWas;
        bool geschlossenDieserFrame;

        public ObjectType? CurrentType => IsOpen ? Types[Index] : null;

        /// <summary>
        /// Palm-Geste: steigende Flanke öffnet, fallende schließt (Grammatik §1 — Handfläche
        /// weg = Typ wählen). Rückgabe: Offen-Status des VORherigen Frames — der Flanken-Frame
        /// selbst gilt noch als alter Zustand (UI blendet einen Frame verzögert ein; HandMenuTests).
        /// </summary>
        public bool UpdatePalm(bool palmFacingBody)
        {
            var before = IsOpen;
            if (palmFacingBody != palmWas)
            {
                IsOpen = !IsOpen;
                if (IsOpen) Index = 0; // nur beim ÖFFNEN zurücksetzen (Fix 16.11.: bei Schließen
                                       // löschte der Reset die Auswahl, bevor sie gelesen wurde)
                geschlossenDieserFrame = before && !IsOpen; // fallende Flanke = Auswahl JETZT
            }
            else geschlossenDieserFrame = false;
            palmWas = palmFacingBody;
            return before;
        }

        /// <summary>
        /// Fix 16.11. (Simulator-Befund): Der bisherige Weg — Close() im Frame NACH der
        /// Fall-Flanke — traf IsOpen bereits wieder zu (UpdatePalm hat den Flip ein Frame
        /// früher gemacht) und lieferte IMMER null → Menü-Auswahl war komplett tot. Die Wahl
        /// wird deshalb IN der Fall-Flanke konsumiert; einmalig pro Flanke.
        /// </summary>
        public ObjectType? ConsumeCloseSelection()
        {
            if (!geschlossenDieserFrame) return null;
            geschlossenDieserFrame = false;
            return Types[Index];
        }

        /// <summary>Typ weiterblättern (Pinch im Menü). Nur wirksam, wenn offen.</summary>
        public void Cycle(int steps)
        {
            if (!IsOpen) return;
            Index = ((Index + steps) % Types.Length + Types.Length) % Types.Length;
        }

        /// <summary>Handfläche weg = Typ wählen. Liefert den Typ — oder null, wenn das Menü zu war.</summary>
        public ObjectType? Close()
        {
            if (!IsOpen) return null;
            IsOpen = false;
            return Types[Index];
        }

        public void Cancel() => IsOpen = false;
    }
}
