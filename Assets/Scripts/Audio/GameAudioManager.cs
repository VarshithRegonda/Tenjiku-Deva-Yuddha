using UnityEngine;

namespace TenjikuDevaYuddha.Core
{
    /// <summary>
    /// Simple audio manager for background music and SFX.
    /// Persists across scenes. Currently procedurally generates ambient tones.
    /// Will be replaced with proper audio assets later.
    /// </summary>
    public class GameAudioManager : MonoBehaviour
    {
        public static GameAudioManager Instance { get; private set; }

        private AudioSource _musicSource;
        private AudioSource _sfxSource;
        private AudioSource _ambientSource;

        [Range(0f, 1f)] public float MusicVolume = 0.5f;
        [Range(0f, 1f)] public float SfxVolume = 1f;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);

            // Create audio sources
            _musicSource = gameObject.AddComponent<AudioSource>();
            _musicSource.loop = true;
            _musicSource.volume = MusicVolume;
            _musicSource.playOnAwake = false;

            _sfxSource = gameObject.AddComponent<AudioSource>();
            _sfxSource.loop = false;
            _sfxSource.volume = SfxVolume;
            _sfxSource.playOnAwake = false;

            _ambientSource = gameObject.AddComponent<AudioSource>();
            _ambientSource.loop = true;
            _ambientSource.volume = 0.3f;
            _ambientSource.playOnAwake = false;

            // Subscribe to game events for SFX
            GameEvents.OnBuildingPlaced += _ => PlayBuildSound();
            GameEvents.OnBuildingCompleted += _ => PlayCompleteSound();
            GameEvents.OnRudraPowerActivated += _ => PlayDivineSound();
            GameEvents.OnAgeChanged += (_, _) => PlayAgeAdvanceSound();
        }

        private void Start()
        {
            // Generate a simple ambient tone using AudioClip.Create
            PlayAmbientTone();
        }

        public void SetMusicVolume(float vol)
        {
            MusicVolume = vol;
            if (_musicSource != null) _musicSource.volume = vol;
        }

        public void SetSfxVolume(float vol)
        {
            SfxVolume = vol;
            if (_sfxSource != null) _sfxSource.volume = vol;
        }

        // ─────────────────────────────────────────────
        //  Procedural Audio (placeholder until real assets)
        // ─────────────────────────────────────────────
        private void PlayAmbientTone()
        {
            // Create a gentle ambient drone
            int sampleRate = 44100;
            int duration = 10; // seconds, will loop
            int samples = sampleRate * duration;
            float[] data = new float[samples];

            for (int i = 0; i < samples; i++)
            {
                float t = (float)i / sampleRate;
                // Gentle OM-like drone: two sine waves
                float tone1 = Mathf.Sin(2f * Mathf.PI * 136.1f * t) * 0.15f; // C#3 (tanpura SA)
                float tone2 = Mathf.Sin(2f * Mathf.PI * 204.15f * t) * 0.08f; // G3 (tanpura PA)
                float tone3 = Mathf.Sin(2f * Mathf.PI * 68.05f * t) * 0.1f;   // C#2 (bass drone)
                // Gentle amplitude modulation for "breathing" effect
                float envelope = 0.7f + 0.3f * Mathf.Sin(2f * Mathf.PI * 0.1f * t);
                data[i] = (tone1 + tone2 + tone3) * envelope;
            }

            AudioClip clip = AudioClip.Create("AmbientDrone", samples, 1, sampleRate, false);
            clip.SetData(data, 0);
            _ambientSource.clip = clip;
            _ambientSource.Play();
        }

        private void PlayBuildSound()
        {
            PlayProceduralSfx(440f, 0.1f, 0.3f);  // short click
        }

        private void PlayCompleteSound()
        {
            PlayProceduralSfx(523.25f, 0.3f, 0.5f); // happy chime
        }

        private void PlayDivineSound()
        {
            PlayProceduralSfx(261.63f, 0.5f, 0.7f); // deep divine tone
        }

        private void PlayAgeAdvanceSound()
        {
            PlayProceduralSfx(329.63f, 0.8f, 0.8f); // triumphant
        }

        private void PlayProceduralSfx(float frequency, float duration, float volume)
        {
            int sampleRate = 44100;
            int samples = (int)(sampleRate * duration);
            float[] data = new float[samples];

            for (int i = 0; i < samples; i++)
            {
                float t = (float)i / sampleRate;
                float envelope = 1f - (t / duration); // fade out
                envelope = envelope * envelope;        // quadratic fade
                data[i] = Mathf.Sin(2f * Mathf.PI * frequency * t) * envelope * volume;
                // Add harmonic for richness
                data[i] += Mathf.Sin(2f * Mathf.PI * frequency * 2f * t) * envelope * volume * 0.3f;
            }

            AudioClip clip = AudioClip.Create("SFX", samples, 1, sampleRate, false);
            clip.SetData(data, 0);
            _sfxSource.PlayOneShot(clip, SfxVolume);
        }
    }
}
