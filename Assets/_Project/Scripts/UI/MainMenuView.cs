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
        [SerializeField] private Button playButton;
        [SerializeField] private Button levelSelectButton;
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
            foreach (Button btn in _spawnedButtons) {
                if (btn != null) {
                    Destroy(btn.gameObject);
                }
            }
            _spawnedButtons.Clear();

            if (levelButtonTemplate == null || levelButtonsContainer == null || levels == null) {
                return;
            }

            for (int i = 0; i < levels.Count; i++) {
                int levelIndex = i;
                LevelConfig cfg = levels[i];
                Button btn = Instantiate(levelButtonTemplate, levelButtonsContainer);
                btn.gameObject.SetActive(true);
                
                TMP_Text label = btn.GetComponentInChildren<TMP_Text>();
                if (label != null) {
                    label.text = (i + 1).ToString();
                    label.fontSize = 32;
                    if (i == currentLevelIndex) {
                        label.color = new Color(1f, 0.9f, 0.2f);
                    } else {
                        label.color = Color.white;
                    }
                }

                btn.onClick.AddListener(() => {
                    if (Audio.AudioManager.Instance != null) {
                        Audio.AudioManager.Instance.PlayUiClick();
                    }
                    LevelSelected?.Invoke(levelIndex);
                    Hide();
                });

                _spawnedButtons.Add(btn);
            }
        }

        private void Awake() {
            if (titleTransform != null) {
                _titleOrigin = titleTransform.anchoredPosition;
            }

            if (playButton != null) {
                playButton.onClick.AddListener(HandlePlayClicked);
            }

            if (levelSelectButton != null) {
                levelSelectButton.onClick.AddListener(HandleOpenLevelSelect);
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
            if (levelSelectPanel != null) {
                levelSelectPanel.SetActive(true);
            }
        }

        private void HandleCloseLevelSelect() {
            if (Audio.AudioManager.Instance != null) {
                Audio.AudioManager.Instance.PlayUiClick();
            }
            if (levelSelectPanel != null) {
                levelSelectPanel.SetActive(false);
            }
        }
    }
}
