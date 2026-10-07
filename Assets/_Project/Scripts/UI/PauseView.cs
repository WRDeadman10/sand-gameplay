using System;

using DG.Tweening;

using SandGameplay.Audio;

using UnityEngine;
using UnityEngine.UI;

namespace SandGameplay.UI {
    /// <summary>
    /// Pause modal: pauses gameplay and gives options to Resume, Restart, open Settings, or return to Main Menu.
    /// Uses smooth DOTween animations for transitions.
    /// </summary>
    public sealed class PauseView : MonoBehaviour {
        [Header("Root")]
        [SerializeField] private GameObject contentRoot;
        [SerializeField] private CanvasGroup canvasGroup;
        [SerializeField] private RectTransform cardTransform;

        [Header("Buttons")]
        [SerializeField] private Button resumeButton;
        [SerializeField] private Button restartButton;
        [SerializeField] private Button settingsButton;
        [SerializeField] private Button menuButton;

        private bool _isOpen;
        private Tween _fadeTween;
        private Tween _scaleTween;

        public event Action ResumeRequested;
        public event Action RestartRequested;
        public event Action SettingsRequested;
        public event Action MenuRequested;

        public bool IsOpen {
            get {
                return _isOpen;
            }
        }

        public void Show() {
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

            if (resumeButton != null) {
                resumeButton.onClick.AddListener(HandleResumeClicked);
            }
            if (restartButton != null) {
                restartButton.onClick.AddListener(HandleRestartClicked);
            }
            if (settingsButton != null) {
                settingsButton.onClick.AddListener(HandleSettingsClicked);
            }
            if (menuButton != null) {
                menuButton.onClick.AddListener(HandleMenuClicked);
            }

            HideImmediate();
        }

        private void OnDestroy() {
            KillTweens();

            if (resumeButton != null) {
                resumeButton.onClick.RemoveListener(HandleResumeClicked);
            }
            if (restartButton != null) {
                restartButton.onClick.RemoveListener(HandleRestartClicked);
            }
            if (settingsButton != null) {
                settingsButton.onClick.RemoveListener(HandleSettingsClicked);
            }
            if (menuButton != null) {
                menuButton.onClick.RemoveListener(HandleMenuClicked);
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
        }

        private void HandleResumeClicked() {
            if (AudioManager.Instance != null) {
                AudioManager.Instance.PlayButtonClick();
            }
            ResumeRequested?.Invoke();
        }

        private void HandleRestartClicked() {
            if (AudioManager.Instance != null) {
                AudioManager.Instance.PlayButtonClick();
            }
            RestartRequested?.Invoke();
        }

        private void HandleSettingsClicked() {
            if (AudioManager.Instance != null) {
                AudioManager.Instance.PlayButtonClick();
            }
            SettingsRequested?.Invoke();
        }

        private void HandleMenuClicked() {
            if (AudioManager.Instance != null) {
                AudioManager.Instance.PlayButtonClick();
            }
            MenuRequested?.Invoke();
        }
    }
}
