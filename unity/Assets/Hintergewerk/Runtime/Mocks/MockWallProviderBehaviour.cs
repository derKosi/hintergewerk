// Hintergewerk — MonoBehaviour-Hülle für MockWallProvider: die Mocks selbst sind bewusst
// plain classes (testbar ohne GameObject); Szenen-Verdrahtung (BauplanWiring, Simulator-Demo)
// braucht aber eine Component. Die Wände liegen als SERIALIZED Feld — sie überleben damit
// Szenen-Speichern/Laden (Fix 09.11.: das reine CLR-Assignment am inneren Mock ging beim
// Szenen-Reload verloren und der Plan zeigte die Default-Wand statt des Raums).
using System.Collections.Generic;
using UnityEngine;
using Hintergewerk.Core;

namespace Hintergewerk.Mocks
{
    public class MockWallProviderBehaviour : MonoBehaviour, IWallProvider
    {
        public List<WallPlane> walls = new()
        {
            new WallPlane { Point = Vector3.zero, Normal = Vector3.forward, Label = "wall" }
        };

        public List<WallPlane> GetWalls() => walls;

        public bool TryFindPlane(Vector3 point, Vector3 direction, float maxDist, out WallPlane plane) =>
            // immer gegen das AKTUELLE Feld (kein Cache — walls kann zur Laufzeit getauscht werden)
            MockWallProvider.TryFindPlane(point, direction, maxDist, walls, out plane);
    }
}
