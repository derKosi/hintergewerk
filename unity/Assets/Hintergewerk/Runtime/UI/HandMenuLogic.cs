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

        public ObjectType? CurrentType => IsOpen ? Types[Index] : null;

        /// <summary>Steigende Flanke der Palm-Geste toggelt (offen/zu). Rückgabe: Ist offen?</summary>
        public bool UpdatePalm(bool palmFacingBody)
        {
            if (palmFacingBody && !palmWas) { IsOpen = !IsOpen; Index = 0; }
            palmWas = palmFacingBody;
            return IsOpen;
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
