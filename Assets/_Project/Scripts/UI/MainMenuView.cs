using System;
using System.Collections.Generic;

using DG.Tweening;

using SandGameplay.Data;

using TMPro;

using UnityEngine;
using UnityEngine.UI;

namespace SandGameplay.UI {
    /// <summary>
    /// Main menu and Level Select modal: allows playing, browsing all 50 levels, and returning to the menu anytime.
    /// Uses smooth DOTween animations for all UI transitions.
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
        private Tween _menuFadeTween;
        private Tween _panelTween;

        public event Action PlayClicked;
        public event Action<int> LevelSelected;
        public event Action SettingsRequested;

        public bool IsOpen {
            get {
                return menuRoot != null && menuRoot.activeSelf;
            }
        }

        public void Show() {
            KillTweens();

            if (menuRoot != null) {
                menuRoot.SetActive(true);
            }

            if (menuCanvasGroup != null) {
                menuCanvasGroup.alpha = 0f;
                menuCanvasGroup.interactable = true;
                menuCanvasGroup.blocksRaycasts = true;
                _menuFadeTween = menuCanvasGroup.DOFade(1f, 0.26f).SetUpdate(true);
            }

            if (titleTransform != null) {
                titleTransform.localScale = Vector3.one * 0.72f;
                titleTransform.DOScale(Vector3.one, 0.35f).SetEase(Ease.OutBack).SetUpdate(true);
            }

            if (buttonsRoot != null) {
                buttonsRoot.SetActive(true);
                buttonsRoot.transform.localScale = Vector3.one * 0.75f;
                buttonsRoot.transform.DOScale(Vector3.one, 0.32f).SetEase(Ease.OutBack).SetUpdate(true);
            }

            if (levelSelectPanel != null) {
                levelSelectPanel.SetActive(false);
            }
        }

        public void Hide() {
            KillTweens();

            if (menuCanvasGroup != null) {
                menuCanvasGroup.interactable = false;
                menuCanvasGroup.blocksRaycasts = false;
                _menuFadeTween = menuCanvasGroup.DOFade(0f, 0.22f).SetUpdate(true).OnComplete(HandleHideComplete);
            } else {
                HandleHideComplete();
            }
        }

        private void HandleHideComplete() {
            if (menuRoot != null) {
                menuRoot.SetActive(false);
            }
        }

        public void PopulateLevelList(IReadOnlyList<LevelConfig> levels, int currentLevelIndex) {
            if (levelButtonTemplate == null || levelButtonsContainer == null || levels == null) {
                return;
            }

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

                TMP_Text label = btn.GetComponentInChildren<TMP_Text>();
                if (label != null) {
                    label.text = (levelIndex + 1).ToString();
                    label.fontSize = 38;
                    label.color = isCurrent ? new Color(1f, 0.95f, 0.35f) : Color.white;
                }

                Transform badge = btn.transform.Find("Badge");
                if (badge != null) {
                    badge.gameObject.SetActive(isCleared);
                }

                Image tileImg = btn.GetComponent<Image>();
                if (tileImg != null) {
                    if (isCleared) {
                        tileImg.color = new Color(0.18f, 0.62f, 0.36f, 1f);
                    } else if (isCurrent) {
                        tileImg.color = new Color(0.92f, 0.62f, 0.15f, 1f);
                    } else {
                        tileImg.color = new Color(0.24f, 0.20f, 0.40f, 1f);
                    }
                }

                btn.onClick.RemoveAllListeners();
                btn.onClick.AddListener(() => {
                    if (Audio.AudioManager.Instance != null) {
                        Audio.AudioManager.Instance.PlayButtonClick();
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
            KillTweens();

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

        private void KillTweens() {
            if (_menuFadeTween != null && _menuFadeTween.IsActive()) {
                _menuFadeTween.Kill();
            }
            if (_panelTween != null && _panelTween.IsActive()) {
                _panelTween.Kill();
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
                Audio.AudioManager.Instance.PlayButtonClick();
            }
            PlayClicked?.Invoke();
            Hide();
        }

        private void HandleOpenLevelSelect() {
            if (Audio.AudioManager.Instance != null) {
                Audio.AudioManager.Instance.PlayButtonClick();
            }

            if (buttonsRoot != null) {
                buttonsRoot.transform.DOScale(Vector3.one * 0.7f, 0.16f).SetEase(Ease.InBack).SetUpdate(true).OnComplete(() => {
                    buttonsRoot.SetActive(false);
                });
            }

            if (levelSelectPanel != null) {
                levelSelectPanel.SetActive(true);
                CanvasGroup cg = levelSelectPanel.GetComponent<CanvasGroup>();
                if (cg == null) {
                    cg = levelSelectPanel.AddComponent<CanvasGroup>();
                }
                cg.alpha = 0f;
                cg.DOFade(1f, 0.25f).SetUpdate(true);
                levelSelectPanel.transform.localScale = Vector3.one * 0.75f;
                _panelTween = levelSelectPanel.transform.DOScale(Vector3.one, 0.35f).SetEase(Ease.OutBack).SetUpdate(true);
            }
        }

        private void HandleSettingsClicked() {
            if (Audio.AudioManager.Instance != null) {
                Audio.AudioManager.Instance.PlayButtonClick();
            }
            SettingsRequested?.Invoke();
        }

        private void HandleCloseLevelSelect() {
            if (Audio.AudioManager.Instance != null) {
                Audio.AudioManager.Instance.PlayButtonClick();
            }

            if (levelSelectPanel != null) {
                CanvasGroup cg = levelSelectPanel.GetComponent<CanvasGroup>();
                if (cg != null) {
                    cg.DOFade(0f, 0.18f).SetUpdate(true);
                }
                _panelTween = levelSelectPanel.transform.DOScale(Vector3.one * 0.75f, 0.18f).SetEase(Ease.InBack).SetUpdate(true).OnComplete(() => {
                    if (levelSelectPanel != null) {
                        levelSelectPanel.SetActive(false);
                    }
                    if (buttonsRoot != null) {
                        buttonsRoot.SetActive(true);
                        buttonsRoot.transform.localScale = Vector3.one * 0.75f;
                        buttonsRoot.transform.DOScale(Vector3.one, 0.3f).SetEase(Ease.OutBack).SetUpdate(true);
                    }
                });
            } else if (buttonsRoot != null) {
                buttonsRoot.SetActive(true);
            }
        }
    }
}
