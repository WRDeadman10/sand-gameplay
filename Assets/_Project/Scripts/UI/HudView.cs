using System;

using TMPro;

using UnityEngine;
using UnityEngine.UI;

namespace SandGameplay.UI {
    /// <summary>Top HUD (level title, restart button) and the bobbing gameplay hint.</summary>
    public sealed class HudView : MonoBehaviour {
        [SerializeField] private Button restartButton;
        [SerializeField] private TMP_Text levelLabel;
        [SerializeField] private CanvasGroup hintGroup;
        [SerializeField] private RectTransform hintRect;
        [SerializeField, Min(0.01f)] private float hintFadeDuration = 0.35f;
        [SerializeField, Min(0f)] private float hintBobAmplitude = 10f;
        [SerializeField, Min(0f)] private float hintBobSpeed = 3f;

        private Vector2 _hintRestPosition;
        private float _hintTargetAlpha;

        public event Action RestartRequested;

        public void SetLevelName(string displayName) {
            levelLabel.text = displayName;
        }

        public void ShowHint() {
            _hintTargetAlpha = 1f;
        }

        public void HideHint() {
            _hintTargetAlpha = 0f;
        }

        private void Awake() {
            if (restartButton == null || levelLabel == null || hintGroup == null || hintRect == null) {
                Debug.LogError("[HudView] Missing UI references.", this);
                enabled = false;
                return;
            }

            _hintRestPosition = hintRect.anchoredPosition;
            hintGroup.alpha = 0f;
            restartButton.onClick.AddListener(HandleRestartClicked);
        }

        private void OnDestroy() {
            if (restartButton != null) {
                restartButton.onClick.RemoveListener(HandleRestartClicked);
            }
        }

        private void Update() {
            hintGroup.alpha = Mathf.MoveTowards(hintGroup.alpha, _hintTargetAlpha, Time.deltaTime / hintFadeDuration);
            if (hintGroup.alpha > 0f) {
                float bob = Mathf.Sin(Time.time * hintBobSpeed) * hintBobAmplitude;
                hintRect.anchoredPosition = _hintRestPosition + new Vector2(0f, bob);
            }
        }

        private void HandleRestartClicked() {
            RestartRequested?.Invoke();
        }
    }
}