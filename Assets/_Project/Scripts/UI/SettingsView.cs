using System;

using DG.Tweening;

using SandGameplay.Audio;

using TMPro;

using UnityEngine;
using UnityEngine.UI;

namespace SandGameplay.UI {
    /// <summary>
    /// Settings modal: controls BGM and SFX volume and mute states.
    /// Can be opened from MainMenu or during gameplay from PauseView.
    /// Uses smooth DOTween animations for transitions.
    /// </summary>
    public sealed class SettingsView : MonoBehaviour {
        [Header("Root")]
        [SerializeField] private GameObject contentRoot;
        [SerializeField] private CanvasGroup canvasGroup;
        [SerializeField] private RectTransform cardTransform;

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
        private bool _isOpen;
        private bool _isUpdatingUi;
        private Tween _fadeTween;
        private Tween _scaleTween;

        public bool IsOpen {
            get {
                return _isOpen;
            }
        }

        public void Show(Action onClose = null) {
            _onCloseCallback = onClose;
            KillTweens();
            _isOpen = true;

            if (contentRoot != null && contentRoot != gameObject) {
                contentRoot.SetActive(true);
            }

            if (cardTransform == null && transform.childCount > 1) {
                cardTransform = transform.Find("Card") as RectTransform;
            }

            if (cardTransform != null) {
                cardTransform.localScale = Vector3.one * 0.72f;
                _scaleTween = cardTransform.DOScale(Vector3.one, 0.35f).SetEase(Ease.OutBack).SetUpdate(true);
            }

            if (canvasGroup != null) {
                canvasGroup.alpha = 0f;
                canvasGroup.interactable = true;
                canvasGroup.blocksRaycasts = true;
                _fadeTween = canvasGroup.DOFade(1f, 0.24f).SetUpdate(true);
            }

            SyncUI();
        }

        public void Hide() {
            KillTweens();

            if (cardTransform == null && transform.childCount > 1) {
                cardTransform = transform.Find("Card") as RectTransform;
            }

            if (canvasGroup != null) {
                canvasGroup.interactable = false;
                canvasGroup.blocksRaycasts = false;
                _fadeTween = canvasGroup.DOFade(0f, 0.2f).SetUpdate(true);
            }

            if (cardTransform != null) {
                _scaleTween = cardTransform.DOScale(Vector3.one * 0.75f, 0.2f).SetEase(Ease.InBack).SetUpdate(true).OnComplete(HandleHideComplete);
            } else {
                HandleHideComplete();
            }
        }

        public void HideImmediate() {
            KillTweens();
            _isOpen = false;
            if (canvasGroup != null) {
                canvasGroup.alpha = 0f;
                canvasGroup.interactable = false;
                canvasGroup.blocksRaycasts = false;
            }
            if (cardTransform != null) {
                cardTransform.localScale = Vector3.one;
            }
            if (contentRoot != null && contentRoot != gameObject) {
                contentRoot.SetActive(false);
            }
        }

        private void Awake() {
            if (cardTransform == null) {
                Transform foundCard = transform.Find("Card");
                if (foundCard != null) {
                    cardTransform = foundCard as RectTransform;
                }
            }

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

            HideImmediate();
        }

        private void OnDestroy() {
            KillTweens();

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

        private void KillTweens() {
            if (_fadeTween != null && _fadeTween.IsActive()) {
                _fadeTween.Kill();
            }
            if (_scaleTween != null && _scaleTween.IsActive()) {
                _scaleTween.Kill();
            }
        }

        private void HandleHideComplete() {
            _isOpen = false;
            if (contentRoot != null && contentRoot != gameObject) {
                contentRoot.SetActive(false);
            }

            Action callback = _onCloseCallback;
            _onCloseCallback = null;
            callback?.Invoke();
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
                AudioManager.Instance.PlayButtonClick();
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
                AudioManager.Instance.PlayButtonClick();
                AudioManager.Instance.ToggleSfxMuted();
                SyncUI();
            }
        }

        private void HandleCloseClicked() {
            if (AudioManager.Instance != null) {
                AudioManager.Instance.PlayButtonClick();
            }
            Hide();
        }
    }
}
