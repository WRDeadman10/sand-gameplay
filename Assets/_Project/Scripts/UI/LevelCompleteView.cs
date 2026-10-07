using System;
using System.Collections;

using UnityEngine;
using UnityEngine.UI;

namespace SandGameplay.UI {
    /// <summary>
    /// "Level Completed!" panel: dim fade, bouncing title, rotating light rays, confetti and a pulsing Play Again
    /// button. The component's GameObject stays active; <c>content</c> is shown and hidden.
    /// </summary>
    public sealed class LevelCompleteView : MonoBehaviour {
        [SerializeField] private GameObject content;
        [SerializeField] private CanvasGroup canvasGroup;
        [SerializeField] private RectTransform title;
        [SerializeField] private RectTransform rays;
        [SerializeField] private RectTransform playAgainRoot;
        [SerializeField] private Button playAgainButton;
        [SerializeField] private ParticleSystem confetti;
        [SerializeField, Min(0.01f)] private float fadeDuration = 0.25f;
        [SerializeField, Min(0.01f)] private float titleDuration = 0.55f;
        [SerializeField] private AnimationCurve titleScale = new AnimationCurve(new Keyframe(0f, 0f), new Keyframe(0.45f, 1.22f), new Keyframe(0.7f, 0.94f), new Keyframe(1f, 1f));
        [SerializeField, Min(0f)] private float buttonDelay = 0.3f;
        [SerializeField, Min(0.01f)] private float buttonDuration = 0.35f;
        [SerializeField] private AnimationCurve buttonScale = new AnimationCurve(new Keyframe(0f, 0f), new Keyframe(0.6f, 1.15f), new Keyframe(1f, 1f));
        [SerializeField] private float raysDegreesPerSecond = 22f;
        [SerializeField, Min(0f)] private float idlePulseStrength = 0.045f;
        [SerializeField, Min(0f)] private float idlePulseSpeed = 4f;

        private Coroutine _routine;
        private bool _isVisible;
        private bool _isButtonIdle;

        public event Action PlayAgainRequested;

        public void Show() {
            StopRoutine();
            content.SetActive(true);
            _isVisible = true;
            _isButtonIdle = false;
            _routine = StartCoroutine(ShowRoutine());
        }

        public void Hide() {
            StopRoutine();
            HideImmediate();
        }

        private void Awake() {
            if (content == null || canvasGroup == null || title == null || rays == null || playAgainRoot == null || playAgainButton == null) {
                Debug.LogError("[LevelCompleteView] Missing UI references.", this);
                enabled = false;
                return;
            }

            playAgainButton.onClick.AddListener(HandlePlayAgainClicked);
            HideImmediate();
        }

        private void OnDestroy() {
            if (playAgainButton != null) {
                playAgainButton.onClick.RemoveListener(HandlePlayAgainClicked);
            }
        }

        private void Update() {
            if (!_isVisible) {
                return;
            }

            rays.Rotate(0f, 0f, -raysDegreesPerSecond * Time.deltaTime);
            if (_isButtonIdle) {
                float pulse = 1f + (Mathf.Sin(Time.time * idlePulseSpeed) * idlePulseStrength);
                playAgainRoot.localScale = new Vector3(pulse, pulse, 1f);
            }
        }

        private IEnumerator ShowRoutine() {
            canvasGroup.alpha = 0f;
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = true;
            title.localScale = Vector3.zero;
            playAgainRoot.localScale = Vector3.zero;
            if (confetti != null) {
                confetti.Clear(true);
                confetti.Play(true);
            }

            float elapsed = 0f;
            while (elapsed < fadeDuration) {
                elapsed += Time.deltaTime;
                canvasGroup.alpha = Mathf.Clamp01(elapsed / fadeDuration);
                yield return null;
            }

            canvasGroup.alpha = 1f;
            elapsed = 0f;
            while (elapsed < titleDuration) {
                elapsed += Time.deltaTime;
                float scale = titleScale.Evaluate(Mathf.Clamp01(elapsed / titleDuration));
                title.localScale = new Vector3(scale, scale, 1f);
                yield return null;
            }

            title.localScale = Vector3.one;
            elapsed = 0f;
            while (elapsed < buttonDelay) {
                elapsed += Time.deltaTime;
                yield return null;
            }

            elapsed = 0f;
            while (elapsed < buttonDuration) {
                elapsed += Time.deltaTime;
                float scale = buttonScale.Evaluate(Mathf.Clamp01(elapsed / buttonDuration));
                playAgainRoot.localScale = new Vector3(scale, scale, 1f);
                yield return null;
            }

            playAgainRoot.localScale = Vector3.one;
            canvasGroup.interactable = true;
            _isButtonIdle = true;
            _routine = null;
        }

        private void HideImmediate() {
            _isVisible = false;
            _isButtonIdle = false;
            canvasGroup.alpha = 0f;
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
            content.SetActive(false);
            if (confetti != null) {
                confetti.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            }
        }

        private void StopRoutine() {
            if (_routine != null) {
                StopCoroutine(_routine);
                _routine = null;
            }
        }

        private void HandlePlayAgainClicked() {
            if (!_isButtonIdle) {
                return;
            }

            PlayAgainRequested?.Invoke();
        }
    }
}