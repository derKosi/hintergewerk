// Hintergewerk — prozedural erzeugter Klick-Ton (kein Audio-Asset nötig):
// bei Aufprall (volumen nach Impuls) und auf Abruf beim Greifen/ Werfen.
using UnityEngine;

namespace Hintergewerk.Adapters
{
    public class CollisionBlip : MonoBehaviour
    {
        AudioSource src;
        float lastPlay;

        void Start()
        {
            src = gameObject.AddComponent<AudioSource>();
            src.clip = MakeBlip();
            src.playOnAwake = false;
            src.spatialBlend = 1f; // 3D-Sound in der Szene
        }

        void OnCollisionEnter(Collision c)
        {
            float impact = c.relativeVelocity.magnitude;
            if (impact < 0.4f || Time.unscaledTime - lastPlay < 0.08f) return;
            Blip(Random.Range(0.85f, 1.1f), Mathf.Clamp01(impact / 3f));
        }

        public void Blip(float pitch, float volume = 0.7f)
        {
            if (src == null) return;
            lastPlay = Time.unscaledTime;
            src.pitch = pitch;
            src.volume = volume;
            src.Play();
        }

        static AudioClip MakeBlip()
        {
            const int sr = 44100;
            int n = (int)(sr * 0.09f);
            var data = new float[n];
            for (int i = 0; i < n; i++)
            {
                float t = (float)i / sr;
                float env = 1f - (float)i / n;
                data[i] = Mathf.Sin(2f * Mathf.PI * 660f * t) * env * 0.6f;
            }
            var clip = AudioClip.Create("hgw_blip", n, 1, sr, false);
            clip.SetData(data, 0);
            return clip;
        }
    }
}
