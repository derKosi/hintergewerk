// Hintergewerk — Szene-Umklemmung auf ECHTE Hände (15.11., Gerätebefund D45).
// Szene 03 (jetzt Start-Szene) war die automatisierte Selbsttest-Szene: DrawController
// saß am FakeHandSource (skriptetes Demo-Szenario) — am Gerät liefen dadurch FAKE-Daten
// („Objekt gespeichert" ohne echte Hand) und echte Gesten wurden ignoriert. Szene 04
// hatte den SceneSwitcher evtl. ebenfalls an der falschen Quelle; Szene 02/03 hatten
// KEINEN Umschalter → Nutzer saß in 03 fest (kein HUD, kein Escapeweg).
// Fix pro Szene (02, 03, 04): OvrHandSource am Rig (Felder: 4 OVRHand/OVRSkeleton-Refs),
// FakeHandSource-Instanzen löschen, DrawController/SceneSwitcher darauf umklemmen,
// SceneSwitcher nachrüsten wo fehlt. Szene 01 (Debug-Würfel) bleibt absichtlich ohne.
// Adapter-Typen (OvrHandSource/FakeHandSource) leben in Assembly-CSharp — aus dieser
// asmdef NICHT referenzierbar → Reflection über Type.GetType("..., Assembly-CSharp").
// Aufruf: unity command script eval "Hintergewerk.EditorTools.HandQuelleFix.FixAlle(); return 0;"
#if UNITY_EDITOR
using System;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Hintergewerk.EditorTools
{
    public static class HandQuelleFix
    {
        const string SzeneOrdner = "Assets/Hintergewerk/Scenes/";
        // Hand-Prefabs aus dem SDK (PackageCache — GUID ist die stabile Adresse)
        const string PrefabGuidLinks = "c6a74181d4000004887343fb87b3c53a"; // OVRCustomHandPrefab_L
        const string PrefabGuidRechts = "e201a24a09e942042b93390d0444d728"; // OVRCustomHandPrefab_R
        static readonly string[] Szenen = { "02_AnkerTest.unity", "03_CaptureAutoTest.unity", "04_Recall.unity" };

        public static string FixAlle()
        {
            var bericht = new System.Text.StringBuilder();
            foreach (var s in Szenen)
            {
                var scene = EditorSceneManager.OpenScene(SzeneOrdner + s, OpenSceneMode.Single);
                bericht.AppendLine("== " + s + ": " + FixOffeneSzene());
                EditorSceneManager.SaveScene(scene);
            }
            var r = bericht.ToString();
            Debug.Log("[HandQuelleFix] Fertig:\n" + r);
            return r;
        }

        static string FixOffeneSzene()
        {
            var bericht = new System.Text.StringBuilder();

            // 1) Echte Hand-Adapter-Quelle am Rig sicherstellen
            var ohsType = Type.GetType("Hintergewerk.Adapters.OvrHandSource, Assembly-CSharp")
                ?? throw new InvalidOperationException("OvrHandSource nicht gefunden (Assembly-CSharp).");
            var hands = UnityEngine.Object.FindObjectsOfType<OVRHand>(true);
            if (hands.Length == 0)
            {
                // 0) Szenen 03/04 haben KEINE Hand-Prefabs (nur „FakeHands"-Test-GO) —
                //    SDK-Hand-Prefabs unter die Rig-Anchors instanziieren (D45-Nachtrag 16.11.)
                var neu = HandPrefabsInstanziieren(bericht);
                if (!neu) return "KEINE OVRHand und keine Hand-Anchors gefunden — Rig fehlt?";
                hands = UnityEngine.Object.FindObjectsOfType<OVRHand>(true);
            }

            OVRHand links = null, rechts = null;
            foreach (var h in hands)
            {
                // v207: OVRHand hat kein HandType-Feld — Seite über die Anchor-Hierarchie
                // (LeftHandAnchor / RightHandAnchor unter TrackingSpace)
                if (IstSeite(h.transform, "left")) { if (links == null) links = h; }
                else if (IstSeite(h.transform, "right")) { if (rechts == null) rechts = h; }
            }
            if (links == null || rechts == null) return "OVRHand unvollständig (links/rechts).";

            var rig = links.transform.root.gameObject;
            var quelle = rig.GetComponent(ohsType) ?? rig.AddComponent(ohsType);
            var soQ = new SerializedObject(quelle);
            soQ.FindProperty("leftHand").objectReferenceValue = links;
            soQ.FindProperty("rightHand").objectReferenceValue = rechts;
            var ls = links.GetComponent<OVRSkeleton>();
            var rs = rechts.GetComponent<OVRSkeleton>();
            if (ls != null) soQ.FindProperty("leftSkeleton").objectReferenceValue = ls;
            if (rs != null) soQ.FindProperty("rightSkeleton").objectReferenceValue = rs;
            soQ.ApplyModifiedPropertiesWithoutUndo();
            bericht.AppendLine($"OvrHandSource am Rig '{rig.name}' (links={links.name}, rechts={rechts.name}, Skeletons: {ls != null}/{rs != null})");

            // 2) FakeHandSource-Instanzen löschen (Script-Demo-Actor hat im echten Raum nichts verloren)
            var fakeType = Type.GetType("Hintergewerk.Adapters.FakeHandSource, Assembly-CSharp");
            if (fakeType != null)
            {
                foreach (var f in UnityEngine.Object.FindObjectsOfType(fakeType, true))
                {
                    bericht.AppendLine("FakeHandSource entfernt: " + f.name);
                    UnityEngine.Object.DestroyImmediate(f);
                }
            }

            // 3) DrawController + SceneSwitcher auf die echte Quelle klemmen
            foreach (var dc in UnityEngine.Object.FindObjectsOfType<Hintergewerk.Capture.DrawController>(true))
            {
                var so = new SerializedObject(dc);
                so.FindProperty("handSourceBehaviour").objectReferenceValue = quelle;
                so.ApplyModifiedPropertiesWithoutUndo();
                bericht.AppendLine("DrawController → echte Hand-Quelle");
            }
            var ssType = typeof(Hintergewerk.UI.SceneSwitcher);
            bool ssWarDa = rig.GetComponent(ssType) != null;
            var ss = rig.GetComponent(ssType) as MonoBehaviour ?? rig.AddComponent(ssType) as MonoBehaviour;
            var soS = new SerializedObject(ss);
            soS.FindProperty("handSourceBehaviour").objectReferenceValue = quelle;
            soS.ApplyModifiedPropertiesWithoutUndo();
            bericht.AppendLine("SceneSwitcher " + (ssWarDa ? "verkabelt" : "nachgerüstet") + " → echte Hand-Quelle");

            // 4) Bone-Bindings der Skeletons nachziehen (Rezept HandBonesFix: das SDK-Prefab
            //    liefert UNGBUNDEN aus — sonst schräge/„dritte" Hände wie am 15.11. vormittags)
            foreach (var skel in UnityEngine.Object.FindObjectsByType<OVRCustomSkeleton>(
                         FindObjectsInactive.Include, FindObjectsSortMode.None))
            {
                skel.TryAutoMapBonesByName();
                EditorUtility.SetDirty(skel);
                int mapped = 0;
                foreach (var b in skel.CustomBones) if (b != null) mapped++;
                bericht.AppendLine($"Bones {skel.gameObject.name}: {mapped}/{skel.CustomBones.Count}");
            }

            return bericht.ToString();
        }

        /// <summary>Hand-Prefabs unter LeftHandAnchor/RightHandAnchor instanziieren.
        /// True, wenn mindestens ein Anchor da war und bestückt wurde.</summary>
        static bool HandPrefabsInstanziieren(System.Text.StringBuilder bericht)
        {
            bool irgendein = false;
            Instanziieren("LeftHandAnchor", PrefabGuidLinks, bericht, ref irgendein);
            Instanziieren("RightHandAnchor", PrefabGuidRechts, bericht, ref irgendein);
            return irgendein;
        }

        static void Instanziieren(string anchorName, string prefabGuid, System.Text.StringBuilder bericht, ref bool irgendein)
        {
            var anchor = GameObject.Find(anchorName);
            if (anchor == null) { bericht.AppendLine("Anchor fehlt: " + anchorName); return; }
            var pfad = AssetDatabase.GUIDToAssetPath(prefabGuid);
            if (string.IsNullOrEmpty(pfad)) { bericht.AppendLine("Prefab-Pfad nicht gefunden (GUID " + prefabGuid + ")"); return; }
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(pfad);
            if (prefab == null) { bericht.AppendLine("Prefab nicht ladbar: " + pfad); return; }
            var instanz = (GameObject)PrefabUtility.InstantiatePrefab(prefab, anchor.transform);
            instanz.name = prefab.name; // ohne "(Clone)" — IstSeite() muss über die Elternkette laufen
            bericht.AppendLine("Instanziiert: " + prefab.name + " unter " + anchorName);
            irgendein = true;
        }

        static bool IstSeite(Transform t, string seite)
        {
            for (var cur = t; cur != null; cur = cur.parent)
                if (cur.name.ToLowerInvariant().Contains(seite)) return true;
            return false;
        }
    }
}
#endif
