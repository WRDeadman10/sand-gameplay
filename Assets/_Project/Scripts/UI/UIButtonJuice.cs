using DG.Tweening;

using SandGameplay.Audio;

using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace SandGameplay.UI {
    /// <summary>
    /// Adds juicy DOTween punch-scale animation and button click audio to UI buttons.
    /// Works with both pointer click events and UGUI Button onClick.
    /// </summary>
    [RequireComponent(typeof(Button))]
    public sealed class UIButtonJuice : MonoBehaviour, IPointerDownHandler, IPointerUpHandler {
        [SerializeField] private float punchScale = -0.1f;
        [SerializeField] private float punchDuration = 0.16f;
        [SerializeField] private int punchVibrato = 8;
        [SerializeField] private float punchElasticity = 1f;

        private Button _button;
        private Vector3 _originalScale;
        private Tweener _punchTween;

        private void Awake() {
            _button = GetComponent<Button>();
            _originalScale = transform.localScale;
            if (_button != null) {
                _button.onClick.AddListener(HandleClicked);
            }
        }

        private void OnDestroy() {
            if (_button != null) {
                _button.onClick.RemoveListener(HandleClicked);
            }
            if (_punchTween != null) {
                _punchTween.Kill();
            }
        }

        public void OnPointerDown(PointerEventData eventData) {
            if (_button != null && !_button.interactable) {
                return;
            }
            transform.DOScale(_originalScale * 0.94f, 0.08f).SetUpdate(true);
        }

        public void OnPointerUp(PointerEventData eventData) {
            if (_button != null && !_button.interactable) {
                return;
            }
            transform.DOScale(_originalScale, 0.12f).SetEase(Ease.OutBack).SetUpdate(true);
        }

        private void HandleClicked() {
            if (AudioManager.Instance != null) {
                AudioManager.Instance.PlayButtonClick();
            }

            if (_punchTween != null) {
                _punchTween.Kill();
                transform.localScale = _originalScale;
            }

            _punchTween = transform.DOPunchScale(new Vector3(punchScale, punchScale, 0f), punchDuration, punchVibrato, punchElasticity).SetUpdate(true);
        }
    }
}
