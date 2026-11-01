// Tests für den generischen Undo-Stapel: LIFO, Labels, Kapazität, leerer Zustand.
using NUnit.Framework;
using Hintergewerk.Capture;

namespace Hintergewerk.Tests
{
    public class UndoStackTests
    {
        [Test]
        public void Lifo_Reihenfolge()
        {
            var stack = new UndoStack();
            var log = "";
            stack.Push("A", () => log += "A");
            stack.Push("B", () => log += "B");
            stack.Push("C", () => log += "C");

            Assert.AreEqual(3, stack.Count);
            Assert.IsTrue(stack.TryUndo(out var l1));
            Assert.AreEqual("C", l1);
            Assert.IsTrue(stack.TryUndo(out var l2));
            Assert.AreEqual("B", l2);
            Assert.IsTrue(stack.TryUndo(out var l3));
            Assert.AreEqual("A", l3);
            Assert.AreEqual("CBA", log); // Undo-Aktionen in gleicher Reihenfolge ausgeführt
        }

        [Test]
        public void Leer_TryUndoFehlschlag()
        {
            var stack = new UndoStack();
            Assert.IsFalse(stack.TryUndo(out _));
            Assert.IsNull(stack.TopLabel);
        }

        [Test]
        public void Kapazität_ÄltesterFälltRaus()
        {
            var stack = new UndoStack(capacity: 2);
            stack.Push("A", () => { });
            stack.Push("B", () => { });
            stack.Push("C", () => { });
            Assert.AreEqual(2, stack.Count);
            Assert.AreEqual("C", stack.TopLabel);
            stack.TryUndo(out var l);
            Assert.AreEqual("C", l);
            stack.TryUndo(out var l2);
            Assert.AreEqual("B", l2); // "A" war bereits rausgefallen
            Assert.IsFalse(stack.TryUndo(out _));
        }

        [Test]
        public void Clear_LeertStapel()
        {
            var stack = new UndoStack();
            stack.Push("A", () => { });
            stack.Clear();
            Assert.AreEqual(0, stack.Count);
            Assert.IsFalse(stack.TryUndo(out _));
        }

        [Test]
        public void Zustand_WirdWirklichZurückgedreht()
        {
            var stack = new UndoStack();
            var liste = new System.Collections.Generic.List<string> { "x" };
            stack.Push("Element löschen", () => liste.RemoveAt(liste.Count - 1));
            liste.Add("y");
            stack.TryUndo(out _);
            Assert.AreEqual(1, liste.Count); // "y" ist weg, "x" bleibt
        }
    }
}
