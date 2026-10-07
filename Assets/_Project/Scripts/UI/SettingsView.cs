using System;

using SandGameplay.Audio;

using TMPro;

using UnityEngine;
using UnityEngine.UI;

namespace SandGameplay.UI {
    /// <summary>
    /// Settings modal: controls BGM and SFX volume and mute states.
    /// Can be opened from MainMenu or during gameplay from PauseView.
    /// </summary>
    public sealed class SettingsView : MonoBehaviour {
        [Header("Root")]
        [SerializeField] private GameObject contentRoot;
        [SerializeField] private CanvasGroup canvasGroup;

        [Header("Music Controls")]
        [SerializeField] private Slider musicSlider;
        [SerializeField] private Button musicToggleBtn;
        [SerializeField] private TMP_Text musicToggleLabel;
        [SerializeField] private Image musicToggleBg;

        [Header("SFX Controls")]
        [SerializeField] private Slider sfxSlider;
        [SerializeField] private Button sfxToggleBtn;
        [SerializeField] private TMP_Text sfxToggleLabel;
        [SerializeField] private Image sfxToggleBg;

        [Header("Close")]
        [SerializeField] private Button closeButton;

        private Action _onCloseCallback;
        private bool _isUpdatingUi;

        public bool IsOpen => canvasGroup != null ? (canvasGroup.alpha > 0.01f && canvasGroup.blocksRaycasts) : (contentRoot != null && contentRoot.activeSelf);

        public void Show(Action onClose = null) {
            _onCloseCallback = onClose;

            if (contentRoot != null && contentRoot != gameObject) {
                contentRoot.SetActive(true);
            }
            if (canvasGroup != null) {
                canvasGroup.alpha = 1f;
                canvasGroup.interactable = true;
                canvasGroup.blocksRaycasts = true;
            }

            SyncUI();
        }

        public void Hide() {
            if (canvasGroup != null) {
                canvasGroup.alpha = 0f;
                canvasGroup.interactable = false;
                canvasGroup.blocksRaycasts = false;
            }
            if (contentRoot != null && contentRoot != gameObject) {
                contentRoot.SetActive(false);
            }

            Action callback = _onCloseCallback;
            _onCloseCallback = null;
            callback?.Invoke();
        }

        private void Awake() {
            if (closeButton != null) {
                closeButton.onClick.AddListener(HandleCloseClicked);
            }

            if (musicSlider != null) {
                musicSlider.onValueChanged.AddListener(HandleMusicSliderChanged);
            }

            if (musicToggleBtn != null) {
                musicToggleBtn.onClick.AddListener(HandleMusicToggleClicked);
            }

            if (sfxSlider != null) {
                sfxSlider.onValueChanged.AddListener(HandleSfxSliderChanged);
            }

            if (sfxToggleBtn != null) {
                sfxToggleBtn.onClick.AddListener(HandleSfxToggleClicked);
            }

            if (AudioManager.Instance != null) {
                AudioManager.Instance.SettingsChanged += SyncUI;
            }

            Hide();
        }

        private void OnDestroy() {
            if (closeButton != null) {
                closeButton.onClick.RemoveListener(HandleCloseClicked);
            }
            if (musicSlider != null) {
                musicSlider.onValueChanged.RemoveListener(HandleMusicSliderChanged);
            }
            if (musicToggleBtn != null) {
                musicToggleBtn.onClick.RemoveListener(HandleMusicToggleClicked);
            }
            if (sfxSlider != null) {
                sfxSlider.onValueChanged.RemoveListener(HandleSfxSliderChanged);
            }
            if (sfxToggleBtn != null) {
                sfxToggleBtn.onClick.RemoveListener(HandleSfxToggleClicked);
            }
            if (AudioManager.Instance != null) {
                AudioManager.Instance.SettingsChanged -= SyncUI;
            }
        }

        private void SyncUI() {
            if (AudioManager.Instance == null) {
                return;
            }

            _isUpdatingUi = true;

            if (musicSlider != null) {
                musicSlider.SetValueWithoutNotify(AudioManager.Instance.MusicVolume);
            }

            bool musicOn = !AudioManager.Instance.IsMusicMuted;
            if (musicToggleLabel != null) {
                musicToggleLabel.text = musicOn ? "ON" : "OFF";
            }
            if (musicToggleBg != null) {
                musicToggleBg.color = musicOn ? new Color(0.18f, 0.65f, 0.36f) : new Color(0.65f, 0.22f, 0.22f);
            }

            if (sfxSlider != null) {
                sfxSlider.SetValueWithoutNotify(AudioManager.Instance.SfxVolume);
            }

            bool sfxOn = !AudioManager.Instance.IsSfxMuted;
            if (sfxToggleLabel != null) {
                sfxToggleLabel.text = sfxOn ? "ON" : "OFF";
            }
            if (sfxToggleBg != null) {
                sfxToggleBg.color = sfxOn ? new Color(0.18f, 0.65f, 0.36f) : new Color(0.65f, 0.22f, 0.22f);
            }

            _isUpdatingUi = false;
        }

        private void HandleMusicSliderChanged(float val) {
            if (_isUpdatingUi || AudioManager.Instance == null) {
                return;
            }
            AudioManager.Instance.SetMusicVolume(val);
            if (val > 0f && AudioManager.Instance.IsMusicMuted) {
                AudioManager.Instance.SetMusicMuted(false);
            }
            SyncUI();
        }

        private void HandleMusicToggleClicked() {
            if (AudioManager.Instance != null) {
                AudioManager.Instance.PlayUiClick();
                AudioManager.Instance.ToggleMusicMuted();
                SyncUI();
            }
        }

        private void HandleSfxSliderChanged(float val) {
            if (_isUpdatingUi || AudioManager.Instance == null) {
                return;
            }
            AudioManager.Instance.SetSfxVolume(val);
            if (val > 0f && AudioManager.Instance.IsSfxMuted) {
                AudioManager.Instance.SetSfxMuted(false);
            }
            SyncUI();
        }

        private void HandleSfxToggleClicked() {
            if (AudioManager.Instance != null) {
                AudioManager.Instance.PlayUiClick();
                AudioManager.Instance.ToggleSfxMuted();
                SyncUI();
            }
        }

        private void HandleCloseClicked() {
            if (AudioManager.Instance != null) {
                AudioManager.Instance.PlayUiClick();
            }
            Hide();
        }
    }
}
