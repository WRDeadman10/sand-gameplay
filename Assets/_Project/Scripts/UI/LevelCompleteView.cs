using System;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace SandGameplay.UI {
    /// <summary>
    /// "Level Completed!" panel: dim fade, bouncing title, rotating light rays, confetti and a pulsing Play Again
    /// button. Uses DOTween animations for transitions.
    /// </summary>
    public sealed class LevelCompleteView : MonoBehaviour {
        [SerializeField] private GameObject content;
        [SerializeField] private CanvasGroup canvasGroup;
        [SerializeField] private RectTransform title;
        [SerializeField] private RectTransform rays;
        [SerializeField] private RectTransform playAgainRoot;
        [SerializeField] private Button playAgainButton;
        [SerializeField] private Button nextLevelButton;
        [SerializeField] private ParticleSystem confetti;
        [SerializeField, Min(0.01f)] private float fadeDuration = 0.25f;
        [SerializeField, Min(0.01f)] private float titleDuration = 0.55f;
        [SerializeField, Min(0f)] private float buttonDelay = 0.3f;
        [SerializeField, Min(0.01f)] private float buttonDuration = 0.35f;
        [SerializeField] private float raysDegreesPerSecond = 22f;
        [SerializeField, Min(0f)] private float idlePulseStrength = 0.045f;
        [SerializeField, Min(0f)] private float idlePulseSpeed = 4f;

        private bool _isVisible;
        private bool _isButtonIdle;
        private Tween _fadeTween;
        private Tween _titleTween;
        private Tween _buttonTween;
        private Tween _pulseTween;

        public event Action PlayAgainRequested;
        public event Action NextLevelRequested;

        public void Show() {
            KillTweens();
            content.SetActive(true);
            _isVisible = true;
            _isButtonIdle = false;

            if (Audio.AudioManager.Instance != null) {
                Audio.AudioManager.Instance.PlayLevelComplete();
            }

            canvasGroup.alpha = 0f;
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = true;

            title.localScale = Vector3.zero;
            playAgainRoot.localScale = Vector3.zero;

            if (confetti != null) {
                confetti.Clear(true);
                confetti.Play(true);
            }

            _fadeTween = canvasGroup.DOFade(1f, fadeDuration).SetUpdate(true);
            _titleTween = title.DOScale(Vector3.one, titleDuration).SetEase(Ease.OutBack).SetUpdate(true);

            _buttonTween = playAgainRoot.DOScale(Vector3.one, buttonDuration)
                .SetDelay(buttonDelay)
                .SetEase(Ease.OutBack)
                .SetUpdate(true)
                .OnComplete(HandleEntranceComplete);
        }

        private void HandleEntranceComplete() {
            canvasGroup.interactable = true;
            _isButtonIdle = true;
            _pulseTween = playAgainRoot.DOScale(Vector3.one * (1f + idlePulseStrength), 1f / idlePulseSpeed)
                .SetEase(Ease.InOutSine)
                .SetLoops(-1, LoopType.Yoyo)
                .SetUpdate(true);
        }

        public void Hide() {
            KillTweens();
            if (canvasGroup != null) {
                canvasGroup.interactable = false;
                canvasGroup.blocksRaycasts = false;
                _fadeTween = canvasGroup.DOFade(0f, 0.2f).SetUpdate(true).OnComplete(HideImmediate);
            } else {
                HideImmediate();
            }
        }

        private void HideImmediate() {
            _isVisible = false;
            _isButtonIdle = false;
            if (canvasGroup != null) {
                canvasGroup.alpha = 0f;
                canvasGroup.interactable = false;
                canvasGroup.blocksRaycasts = false;
            }
            if (content != null) {
                content.SetActive(false);
            }
            if (confetti != null) {
                confetti.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            }
        }

        private void KillTweens() {
            if (_fadeTween != null && _fadeTween.IsActive()) {
                _fadeTween.Kill();
            }
            if (_titleTween != null && _titleTween.IsActive()) {
                _titleTween.Kill();
            }
            if (_buttonTween != null && _buttonTween.IsActive()) {
                _buttonTween.Kill();
            }
            if (_pulseTween != null && _pulseTween.IsActive()) {
                _pulseTween.Kill();
            }
        }

        private void Awake() {
            if (content == null || canvasGroup == null || title == null || rays == null || playAgainRoot == null) {
                Debug.LogError("[LevelCompleteView] Missing UI references.", this);
                enabled = false;
                return;
            }

            if (nextLevelButton != null) {
                nextLevelButton.onClick.AddListener(HandleNextLevelClicked);
            }

            if (playAgainButton != null && playAgainButton != nextLevelButton) {
                playAgainButton.onClick.AddListener(HandlePlayAgainClicked);
            } else if (playAgainButton != null && nextLevelButton == null) {
                nextLevelButton = playAgainButton;
                nextLevelButton.onClick.AddListener(HandleNextLevelClicked);
            }

            HideImmediate();
        }

        private void OnDestroy() {
            KillTweens();

            if (nextLevelButton != null) {
                nextLevelButton.onClick.RemoveListener(HandleNextLevelClicked);
            }
            if (playAgainButton != null && playAgainButton != nextLevelButton) {
                playAgainButton.onClick.RemoveListener(HandlePlayAgainClicked);
            }
        }

        private void Update() {
            if (!_isVisible) {
                return;
            }

            if (rays != null) {
                rays.Rotate(0f, 0f, -raysDegreesPerSecond * Time.deltaTime);
            }
        }

        private void HandlePlayAgainClicked() {
            if (!_isButtonIdle) {
                return;
            }

            if (Audio.AudioManager.Instance != null) {
                Audio.AudioManager.Instance.PlayButtonClick();
            }

            PlayAgainRequested?.Invoke();
        }

        private void HandleNextLevelClicked() {
            if (!_isButtonIdle) {
                return;
            }

            if (Audio.AudioManager.Instance != null) {
                Audio.AudioManager.Instance.PlayButtonClick();
            }

            NextLevelRequested?.Invoke();
        }
    }
}