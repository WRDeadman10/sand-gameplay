using System;
using System.Collections.Generic;

using SandGameplay.Data;

using TMPro;

using UnityEngine;
using UnityEngine.UI;

namespace SandGameplay.UI {
    /// <summary>
    /// Main menu and Level Select modal: allows playing, browsing all 11 levels, and returning to the menu anytime.
    /// </summary>
    public sealed class MainMenuView : MonoBehaviour {
        [Header("Root")]
        [SerializeField] private GameObject menuRoot;
        [SerializeField] private CanvasGroup menuCanvasGroup;

        [Header("Buttons")]
        [SerializeField] private GameObject buttonsRoot;
        [SerializeField] private Button playButton;
        [SerializeField] private Button levelSelectButton;
        [SerializeField] private Button settingsButton;
        [SerializeField] private Button closeSelectButton;

        [Header("Level Select Panel")]
        [SerializeField] private GameObject levelSelectPanel;
        [SerializeField] private Transform levelButtonsContainer;
        [SerializeField] private Button levelButtonTemplate;

        [Header("Floating Animation")]
        [SerializeField] private RectTransform titleTransform;
        [SerializeField, Min(0f)] private float titleFloatSpeed = 2f;
        [SerializeField, Min(0f)] private float titleFloatAmount = 8f;

        private Vector2 _titleOrigin;
        private readonly List<Button> _spawnedButtons = new List<Button>();

        public event Action PlayClicked;
        public event Action<int> LevelSelected;
        public event Action SettingsRequested;

        public bool IsOpen => menuRoot != null && menuRoot.activeSelf;

        public void Show() {
            if (menuRoot != null) {
                menuRoot.SetActive(true);
            }
            if (menuCanvasGroup != null) {
                menuCanvasGroup.alpha = 1f;
                menuCanvasGroup.interactable = true;
                menuCanvasGroup.blocksRaycasts = true;
            }
            if (buttonsRoot != null) {
                buttonsRoot.SetActive(true);
            }
            if (levelSelectPanel != null) {
                levelSelectPanel.SetActive(false);
            }
        }

        public void Hide() {
            if (menuCanvasGroup != null) {
                menuCanvasGroup.alpha = 0f;
                menuCanvasGroup.interactable = false;
                menuCanvasGroup.blocksRaycasts = false;
            }
            if (menuRoot != null) {
                menuRoot.SetActive(false);
            }
        }

        public void PopulateLevelList(IReadOnlyList<LevelConfig> levels, int currentLevelIndex) {
            if (levelButtonTemplate == null || levelButtonsContainer == null || levels == null) {
                return;
            }

            // Remove any unmanaged or null buttons
            for (int i = levelButtonsContainer.childCount - 1; i >= 0; i--) {
                Transform child = levelButtonsContainer.GetChild(i);
                if (child == levelButtonTemplate.transform) {
                    continue;
                }
                Button btnComp = child.GetComponent<Button>();
                if (btnComp == null || !_spawnedButtons.Contains(btnComp)) {
                    child.SetParent(null);
                    Destroy(child.gameObject);
                }
            }

            // Pool buttons: only create if we have fewer than levels.Count
            while (_spawnedButtons.Count < levels.Count) {
                Button newBtn = Instantiate(levelButtonTemplate, levelButtonsContainer);
                _spawnedButtons.Add(newBtn);
            }

            for (int i = 0; i < _spawnedButtons.Count; i++) {
                Button btn = _spawnedButtons[i];
                if (i >= levels.Count) {
                    btn.gameObject.SetActive(false);
                    continue;
                }

                btn.gameObject.SetActive(true);
                int levelIndex = i;

                bool isCleared = Core.LevelProgress.IsLevelCleared(levelIndex);
                bool isCurrent = (levelIndex == currentLevelIndex);

                // Only show the level number
                TMP_Text label = btn.GetComponentInChildren<TMP_Text>();
                if (label != null) {
                    label.text = (levelIndex + 1).ToString();
                    label.fontSize = 38;
                    label.color = isCurrent ? new Color(1f, 0.95f, 0.35f) : Color.white;
                }

                // Checkmark badge for cleared levels
                Transform badge = btn.transform.Find("Badge");
                if (badge != null) {
                    badge.gameObject.SetActive(isCleared);
                }

                // Tile background styling
                Image tileImg = btn.GetComponent<Image>();
                if (tileImg != null) {
                    if (isCleared) {
                        tileImg.color = new Color(0.18f, 0.62f, 0.36f, 1f); // Emerald green for cleared
                    } else if (isCurrent) {
                        tileImg.color = new Color(0.92f, 0.62f, 0.15f, 1f); // Vibrant gold for current level
                    } else {
                        tileImg.color = new Color(0.24f, 0.20f, 0.40f, 1f); // Default dark tile
                    }
                }

                btn.onClick.RemoveAllListeners();
                btn.onClick.AddListener(() => {
                    if (Audio.AudioManager.Instance != null) {
                        Audio.AudioManager.Instance.PlayUiClick();
                    }
                    LevelSelected?.Invoke(levelIndex);
                    Hide();
                });
            }
        }

        private void Awake() {
            if (buttonsRoot == null && playButton != null) {
                buttonsRoot = playButton.transform.parent != null ? playButton.transform.parent.gameObject : null;
            }

            if (titleTransform != null) {
                _titleOrigin = titleTransform.anchoredPosition;
            }

            if (playButton != null) {
                playButton.onClick.AddListener(HandlePlayClicked);
            }

            if (levelSelectButton != null) {
                levelSelectButton.onClick.AddListener(HandleOpenLevelSelect);
            }

            if (settingsButton != null) {
                settingsButton.onClick.AddListener(HandleSettingsClicked);
            }

            if (closeSelectButton != null) {
                closeSelectButton.onClick.AddListener(HandleCloseLevelSelect);
            }

            if (levelButtonTemplate != null) {
                levelButtonTemplate.gameObject.SetActive(false);
            }
        }

        private void OnDestroy() {
            if (playButton != null) {
                playButton.onClick.RemoveListener(HandlePlayClicked);
            }
            if (levelSelectButton != null) {
                levelSelectButton.onClick.RemoveListener(HandleOpenLevelSelect);
            }
            if (settingsButton != null) {
                settingsButton.onClick.RemoveListener(HandleSettingsClicked);
            }
            if (closeSelectButton != null) {
                closeSelectButton.onClick.RemoveListener(HandleCloseLevelSelect);
            }
        }

        private void Update() {
            if (titleTransform != null && IsOpen) {
                float offset = Mathf.Sin(Time.time * titleFloatSpeed) * titleFloatAmount;
                titleTransform.anchoredPosition = _titleOrigin + new Vector2(0f, offset);
            }
        }

        private void HandlePlayClicked() {
            if (Audio.AudioManager.Instance != null) {
                Audio.AudioManager.Instance.PlayUiClick();
            }
            PlayClicked?.Invoke();
            Hide();
        }

        private void HandleOpenLevelSelect() {
            if (Audio.AudioManager.Instance != null) {
                Audio.AudioManager.Instance.PlayUiClick();
            }
            if (buttonsRoot != null) {
                buttonsRoot.SetActive(false);
            }
            if (levelSelectPanel != null) {
                levelSelectPanel.SetActive(true);
            }
        }

        private void HandleSettingsClicked() {
            if (Audio.AudioManager.Instance != null) {
                Audio.AudioManager.Instance.PlayUiClick();
            }
            SettingsRequested?.Invoke();
        }

        private void HandleCloseLevelSelect() {
            if (Audio.AudioManager.Instance != null) {
                Audio.AudioManager.Instance.PlayUiClick();
            }
            if (levelSelectPanel != null) {
                levelSelectPanel.SetActive(false);
            }
            if (buttonsRoot != null) {
                buttonsRoot.SetActive(true);
            }
        }
    }
}
