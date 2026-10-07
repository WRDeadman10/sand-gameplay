using UnityEngine;

namespace SandGameplay.Audio {
    /// <summary>
    /// Central audio manager that plays one-shot sound effects and loops the sand pour audio.
    /// Accessible anywhere via AudioManager.Instance.
    /// </summary>
    public sealed class AudioManager : MonoBehaviour {
        public static AudioManager Instance { get; private set; }

        [Header("Audio Sources")]
        [SerializeField] private AudioSource sfxSource;
        [SerializeField] private AudioSource pourSource;

        [Header("Clips")]
        [SerializeField] private AudioClip uiClickClip;
        [SerializeField] private AudioClip dockClip;
        [SerializeField] private AudioClip sandPourClip;
        [SerializeField] private AudioClip boxPopClip;
        [SerializeField] private AudioClip praiseClip;
        [SerializeField] private AudioClip levelCompleteClip;

        private bool _isPouring;

        private void Awake() {
            if (Instance != null && Instance != this) {
                Destroy(gameObject);
                return;
            }

            Instance = this;

            if (sfxSource == null) {
                sfxSource = gameObject.AddComponent<AudioSource>();
                sfxSource.playOnAwake = false;
            }

            if (pourSource == null) {
                pourSource = gameObject.AddComponent<AudioSource>();
                pourSource.playOnAwake = false;
                pourSource.loop = true;
            }

            if (sandPourClip != null) {
                pourSource.clip = sandPourClip;
            }
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

            if (pouring) {
                if (!pourSource.isPlaying) {
                    pourSource.volume = 0.65f;
                    pourSource.Play();
                }
            } else {
                if (pourSource.isPlaying) {
                    pourSource.Stop();
                }
            }
        }

        private void PlayOneShot(AudioClip clip, float volume = 1f) {
            if (clip != null && sfxSource != null) {
                sfxSource.PlayOneShot(clip, volume);
            }
        }
    }
}
