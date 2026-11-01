// Hintergewerk — generischer Undo-Stapel (Grammatik §1: „Wischen = rückgängig").
// Closures statt Command-Klassen: klein, testbar, ausreichend für den MVP.
// Der Controller capture't beim Push, was rückgängig zu machen ist; Undo dreht es zurück.
// Kapazität begrenzt (ältester Eintrag fällt zuerst heraus).
using System;
using System.Collections.Generic;

namespace Hintergewerk.Capture
{
    public class UndoStack
    {
        readonly List<(string Label, Action Undo)> items = new();
        readonly int capacity;

        public int Count => items.Count;
        public string TopLabel => items.Count == 0 ? null : items[items.Count - 1].Label;

        public UndoStack(int capacity = 50) { this.capacity = Math.Max(1, capacity); }

        public void Push(string label, Action undo)
        {
            if (undo == null) return;
            items.Add((label, undo));
            if (items.Count > capacity) items.RemoveAt(0);
        }

        public bool TryUndo(out string label)
        {
            label = null;
            if (items.Count == 0) return false;
            var last = items[items.Count - 1];
            items.RemoveAt(items.Count - 1);
            label = last.Label;
            last.Undo();
            return true;
        }

        public void Clear() => items.Clear();
    }
}
