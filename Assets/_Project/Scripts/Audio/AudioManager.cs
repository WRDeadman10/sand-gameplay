using System;

using UnityEngine;

namespace SandGameplay.Audio {
    /// <summary>
    /// Central audio manager that plays one-shot sound effects, loops the sand pour audio,
    /// and plays looping cute background music with volume and mute controls.
    /// Accessible anywhere via AudioManager.Instance.
    /// </summary>
    public sealed class AudioManager : MonoBehaviour {
        public static AudioManager Instance { get; private set; }

        private const string PrefMusicVolume = "SG_MusicVolume";
        private const string PrefMusicMuted = "SG_MusicMuted";
        private const string PrefSfxVolume = "SG_SfxVolume";
        private const string PrefSfxMuted = "SG_SfxMuted";

        [Header("Audio Sources")]
        [SerializeField] private AudioSource sfxSource;
        [SerializeField] private AudioSource pourSource;
        [SerializeField] private AudioSource musicSource;

        [Header("Music")]
        [SerializeField] private AudioClip bgmClip;

        [Header("SFX Clips")]
        [SerializeField] private AudioClip uiClickClip;
        [SerializeField] private AudioClip dockClip;
        [SerializeField] private AudioClip sandPourClip;
        [SerializeField] private AudioClip boxPopClip;
        [SerializeField] private AudioClip praiseClip;
        [SerializeField] private AudioClip levelCompleteClip;

        public float MusicVolume { get; private set; } = 0.6f;
        public bool IsMusicMuted { get; private set; } = false;
        public float SfxVolume { get; private set; } = 0.85f;
        public bool IsSfxMuted { get; private set; } = false;

        public event Action SettingsChanged;

        private bool _isPouring;

        private void Awake() {
            if (Instance != null && Instance != this) {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            LoadPreferences();

            if (sfxSource == null) {
                sfxSource = gameObject.AddComponent<AudioSource>();
                sfxSource.playOnAwake = false;
            }

            if (pourSource == null) {
                pourSource = gameObject.AddComponent<AudioSource>();
                pourSource.playOnAwake = false;
                pourSource.loop = true;
            }

            if (musicSource == null) {
                musicSource = gameObject.AddComponent<AudioSource>();
                musicSource.playOnAwake = false;
                musicSource.loop = true;
            }

            if (sandPourClip != null) {
                pourSource.clip = sandPourClip;
            }

            if (bgmClip == null) {
#if UNITY_EDITOR
                bgmClip = UnityEditor.AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/_Project/Audio/bgm_cute_loop.wav");
#endif
            }

            if (bgmClip != null) {
                musicSource.clip = bgmClip;
            }

            ApplyMusicSettings();
        }

        private void Start() {
            StartMusic();
        }

        private void LoadPreferences() {
            MusicVolume = PlayerPrefs.GetFloat(PrefMusicVolume, 0.6f);
            IsMusicMuted = PlayerPrefs.GetInt(PrefMusicMuted, 0) == 1;
            SfxVolume = PlayerPrefs.GetFloat(PrefSfxVolume, 0.85f);
            IsSfxMuted = PlayerPrefs.GetInt(PrefSfxMuted, 0) == 1;
        }

        private void SavePreferences() {
            PlayerPrefs.SetFloat(PrefMusicVolume, MusicVolume);
            PlayerPrefs.SetInt(PrefMusicMuted, IsMusicMuted ? 1 : 0);
            PlayerPrefs.SetFloat(PrefSfxVolume, SfxVolume);
            PlayerPrefs.SetInt(PrefSfxMuted, IsSfxMuted ? 1 : 0);
            PlayerPrefs.Save();
        }

        public void SetMusicVolume(float volume) {
            MusicVolume = Mathf.Clamp01(volume);
            ApplyMusicSettings();
            SavePreferences();
            SettingsChanged?.Invoke();
        }

        public void SetMusicMuted(bool muted) {
            IsMusicMuted = muted;
            ApplyMusicSettings();
            SavePreferences();
            SettingsChanged?.Invoke();
        }

        public void ToggleMusicMuted() {
            SetMusicMuted(!IsMusicMuted);
        }

        public void SetSfxVolume(float volume) {
            SfxVolume = Mathf.Clamp01(volume);
            SavePreferences();
            SettingsChanged?.Invoke();
        }

        public void SetSfxMuted(bool muted) {
            IsSfxMuted = muted;
            if (IsSfxMuted && _isPouring && pourSource != null) {
                pourSource.Stop();
            }
            SavePreferences();
            SettingsChanged?.Invoke();
        }

        public void ToggleSfxMuted() {
            SetSfxMuted(!IsSfxMuted);
        }

        public void StartMusic() {
            if (musicSource == null || musicSource.clip == null) {
                return;
            }
            if (!musicSource.isPlaying) {
                musicSource.Play();
            }
            ApplyMusicSettings();
        }

        private void ApplyMusicSettings() {
            if (musicSource == null) return;
            musicSource.volume = IsMusicMuted ? 0f : MusicVolume;
        }

        public void PlayUiClick() {
            PlayOneShot(uiClickClip, 0.7f);
        }

        public void PlayDock() {
            PlayOneShot(dockClip, 0.8f);
        }

        public void PlayBoxPop() {
            PlayOneShot(boxPopClip, 0.9f);
        }

        public void PlayPraise() {
            PlayOneShot(praiseClip, 0.85f);
        }

        public void PlayLevelComplete() {
            PlayOneShot(levelCompleteClip, 0.9f);
        }

        public void SetPouring(bool pouring) {
            if (_isPouring == pouring) {
                return;
            }

            _isPouring = pouring;
            if (pourSource == null || pourSource.clip == null) {
                return;
            }

            if (pouring && !IsSfxMuted) {
                if (!pourSource.isPlaying) {
                    pourSource.volume = 0.65f * SfxVolume;
                    pourSource.Play();
                }
            } else {
                if (pourSource.isPlaying) {
                    pourSource.Stop();
                }
            }
        }

        private void PlayOneShot(AudioClip clip, float volume = 1f) {
            if (IsSfxMuted) return;
            if (clip != null && sfxSource != null) {
                sfxSource.PlayOneShot(clip, volume * SfxVolume);
            }
        }
    }
}
