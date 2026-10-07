using SandGameplay.Container;

using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

namespace SandGameplay.Interaction {
    /// <summary>
    /// Pointer (mouse, pen or touch) drag of the box. Polls the Gameplay actions every frame so one code path
    /// serves every pointer type; presses that start over UI are ignored.
    /// </summary>
    [DefaultExecutionOrder(-100)]
    public sealed class ContainerDragController : MonoBehaviour {
        [SerializeField] private InputActionReference pointerPosition;
        [SerializeField] private InputActionReference pointerPress;
        [SerializeField] private Camera worldCamera;
        [SerializeField] private ContainerPiece container;
        [SerializeField, Min(0f), Tooltip("Extra hit area around the box, in board cells.")] private float touchPadding = 0.25f;

        private bool _hasReferences;
        private bool _isInputEnabled;
        private bool _isDragging;
        private Vector2 _grabOffset;

        public void SetInputEnabled(bool inputEnabled) {
            _isInputEnabled = inputEnabled;
            if (!inputEnabled) {
                EndDrag();
            }
        }

        private void Awake() {
            _hasReferences = pointerPosition != null && pointerPosition.action != null && pointerPress != null && pointerPress.action != null && worldCamera != null && container != null;
            if (!_hasReferences) {
                Debug.LogError("[ContainerDragController] Input actions, camera and container must be assigned.", this);
                enabled = false;
            }
        }

        private void OnEnable() {
            if (!_hasReferences) {
                return;
            }

            pointerPosition.action.Enable();
            pointerPress.action.Enable();
        }

        private void OnDisable() {
            EndDrag();
        }

        private void Update() {
            if (!_isInputEnabled) {
                return;
            }

            InputAction press = pointerPress.action;
            if (press.WasPressedThisFrame()) {
                TryBeginDrag();
            }

            if (!_isDragging) {
                return;
            }

            if (press.WasReleasedThisFrame() || !press.IsPressed()) {
                EndDrag();
                return;
            }

            container.DragTo(ReadPointerWorld() - _grabOffset);
        }

        private void TryBeginDrag() {
            if (IsPointerOverUi()) {
                return;
            }

            Vector2 pointerWorld = ReadPointerWorld();
            if (!container.ContainsPoint(pointerWorld, touchPadding)) {
                return;
            }

            _grabOffset = pointerWorld - container.Position;
            _isDragging = true;
            container.BeginDrag();
        }

        private void EndDrag() {
            if (!_isDragging) {
                return;
            }

            _isDragging = false;
            container.EndDrag();
        }

        private Vector2 ReadPointerWorld() {
            Vector2 screen = pointerPosition.action.ReadValue<Vector2>();
            Vector3 world = worldCamera.ScreenToWorldPoint(new Vector3(screen.x, screen.y, -worldCamera.transform.position.z));
            return new Vector2(world.x, world.y);
        }

        private static bool IsPointerOverUi() {
            EventSystem eventSystem = EventSystem.current;
            return eventSystem != null && eventSystem.IsPointerOverGameObject();
        }
    }
}