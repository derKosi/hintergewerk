// Hintergewerk — Audio-Ping für Warn-Eskalation (docs/03 §2.5: „visuelle Eskalation +
// räumlicher Audio-Ping"). Prozedural erzeugte Sinus-Töne — kein Audio-Asset im Repo,
// kein SDK. 2D-Wiedergabe reicht für M3 (FoV-aware-Panning: Politur 11.–13.11.).
using UnityEngine;

namespace Hintergewerk.Recall
{
    public static class WarningAudio
    {
        static AudioSource source;
        static AudioClip nearClip;
        static AudioClip criticalClip;

        public static void Ping(WarningLevel level)
        {
            EnsureSource();
            if (source == null) return;
            source.PlayOneShot(level == WarningLevel.Critical ? CriticalClip() : NearClip());
        }

        static void EnsureSource()
        {
            if (source != null) return;
            var go = new GameObject("WarningAudio");
            source = go.AddComponent<AudioSource>();
            source.spatialBlend = 0f; // 2D
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
