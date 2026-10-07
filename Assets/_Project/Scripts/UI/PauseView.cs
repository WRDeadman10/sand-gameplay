using System;

using SandGameplay.Audio;

using UnityEngine;
using UnityEngine.UI;

namespace SandGameplay.UI {
    /// <summary>
    /// Pause modal: pauses gameplay and gives options to Resume, Restart, open Settings, or return to Main Menu.
    /// </summary>
    public sealed class PauseView : MonoBehaviour {
        [Header("Root")]
        [SerializeField] private GameObject contentRoot;
        [SerializeField] private CanvasGroup canvasGroup;

        [Header("Buttons")]
        [SerializeField] private Button resumeButton;
        [SerializeField] private Button restartButton;
        [SerializeField] private Button settingsButton;
        [SerializeField] private Button menuButton;

        public event Action ResumeRequested;
        public event Action RestartRequested;
        public event Action SettingsRequested;
        public event Action MenuRequested;

        public bool IsOpen => canvasGroup != null ? (canvasGroup.alpha > 0.01f && canvasGroup.blocksRaycasts) : (contentRoot != null && contentRoot.activeSelf);

        public void Show() {
            if (contentRoot != null && contentRoot != gameObject) {
                contentRoot.SetActive(true);
            }
            if (canvasGroup != null) {
                canvasGroup.alpha = 1f;
                canvasGroup.interactable = true;
                canvasGroup.blocksRaycasts = true;
            }
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
        }

        private void Awake() {
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

            Hide();
        }

        private void OnDestroy() {
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

        private void HandleResumeClicked() {
            if (AudioManager.Instance != null) {
                AudioManager.Instance.PlayUiClick();
            }
            ResumeRequested?.Invoke();
        }

        private void HandleRestartClicked() {
            if (AudioManager.Instance != null) {
                AudioManager.Instance.PlayUiClick();
            }
            RestartRequested?.Invoke();
        }

        private void HandleSettingsClicked() {
            if (AudioManager.Instance != null) {
                AudioManager.Instance.PlayUiClick();
            }
            SettingsRequested?.Invoke();
        }

        private void HandleMenuClicked() {
            if (AudioManager.Instance != null) {
                AudioManager.Instance.PlayUiClick();
            }
            MenuRequested?.Invoke();
        }
    }
}
