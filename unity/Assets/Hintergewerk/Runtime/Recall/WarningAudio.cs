// Hintergewerk — Audio-Ping für Warn-Eskalation (docs/03 §2.5: „visuelle Eskalation +
// räumlicher Audio-Ping"). Prozedural erzeugte Sinus-Töne — kein Audio-Asset im Repo,
// kein SDK. Politur 08.11. (FoV-aware): Ping pannet zur Gefahr-Mitte, damit der Monteur
// SPÜRT, auf welcher Seite die Leitung liegt — auch wenn sie außerhalb des Blicks ist.
using UnityEngine;

namespace Hintergewerk.Recall
{
    public static class WarningAudio
    {
        static AudioSource source;
        static AudioClip nearClip;
        static AudioClip criticalClip;

        public static void Ping(WarningLevel level) => Ping(level, 0f);

        /// <summary>Ping mit Stereo-Panning (−1 links … +1 rechts, 0 = Mitte).</summary>
        public static void Ping(WarningLevel level, float pan)
        {
            EnsureSource();
            if (source == null) return;
            source.panStereo = Mathf.Clamp(pan, -1f, 1f);
            source.PlayOneShot(level == WarningLevel.Critical ? CriticalClip() : NearClip());
        }

        /// <summary>Panning aus Kopfframe: Projektion der Gefahr-Richtung auf die Kopfrechte.
        /// Rein rechnerisch (testbar) — keine Audio-Hardware nötig.</summary>
        public static float PanFor(Vector3 headPos, Vector3 headRight, Vector3 hazardPos)
        {
            var delta = hazardPos - headPos;
            if (delta.sqrMagnitude < 1e-6f) return 0f;
            var right = headRight.sqrMagnitude < 1e-6f ? Vector3.right : headRight.normalized;
            return Mathf.Clamp(Vector3.Dot(delta.normalized, right), -1f, 1f);
        }

        static void EnsureSource()
        {
            if (source != null) return;
            var go = new GameObject("WarningAudio");
            source = go.AddComponent<AudioSource>();
            source.spatialBlend = 0f; // 2D mit panStereo — richtungsgetreu ohne Spatializer-Komplexität
            source.playOnAwake = false;
        }

        /// <summary>Nähe: kurzer, mittlerer Ton (660 Hz, 0,12 s).</summary>
        static AudioClip NearClip()
        {
            if (nearClip == null) nearClip = Build("HGW_Near", 660f, 0.12f);
            return nearClip;
        }

        /// <summary>Kritisch: höherer, längerer Ton (990 Hz, 0,2 s) — Unmissverständlich.</summary>
        static AudioClip CriticalClip()
        {
            if (criticalClip == null) criticalClip = Build("HGW_Critical", 990f, 0.2f);
            return criticalClip;
        }

        static AudioClip Build(string name, float freq, float seconds)
        {
            int rate = 22050;
            int n = Mathf.CeilToInt(seconds * rate);
            var clip = AudioClip.Create(name, n, 1, rate, false);
            var data = new float[n];
            for (int i = 0; i < n; i++)
            {
                var envelope = 1f - (float)i / n; // ausklingen, kein Knacks
                data[i] = Mathf.Sin(2f * Mathf.PI * freq * i / rate) * envelope * 0.35f;
            }
            clip.SetData(data, 0);
            return clip;
        }
    }
}
