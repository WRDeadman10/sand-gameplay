using System;
using System.Collections;

using SandGameplay.Container;
using SandGameplay.Source;
using SandGameplay.Utilities;

using UnityEngine;

namespace SandGameplay.Presentation {
    /// <summary>
    /// Box completion beats: squash-and-stretch pop with a flash, praise word and star burst, then the box
    /// shrinks, flies into the tank and pops with a puff. The callback then opens the end panel.
    /// </summary>
    public sealed class CompletionSequencer : MonoBehaviour {
        [SerializeField] private PraisePopup praisePopup;
        [SerializeField] private ParticleSystem starBurst;
        [SerializeField] private ParticleSystem sparkleBurst;
        [SerializeField] private ParticleSystem puffBurst;
        [SerializeField] private string[] praiseWords = new string[] { "PERFECT!", "AWESOME!", "GREAT!", "NICE!" };
        [SerializeField] private Color praiseColor = new Color(1f, 0.93f, 0.45f, 1f);
        [SerializeField] private Vector3 praiseOffset = new Vector3(0f, 1.3f, 0f);
        [SerializeField, Min(0.01f)] private float popDuration = 0.3f;
        [SerializeField] private AnimationCurve popScaleX = new AnimationCurve(new Keyframe(0f, 1f), new Keyframe(0.3f, 1.2f), new Keyframe(0.62f, 0.94f), new Keyframe(1f, 1f));
        [SerializeField] private AnimationCurve popScaleY = new AnimationCurve(new Keyframe(0f, 1f), new Keyframe(0.3f, 0.86f), new Keyframe(0.62f, 1.06f), new Keyframe(1f, 1f));
        [SerializeField] private AnimationCurve flashAlpha = new AnimationCurve(new Keyframe(0f, 0f), new Keyframe(0.2f, 0.85f), new Keyframe(1f, 0f));
        [SerializeField, Min(0f)] private float flyDelay = 0.12f;
        [SerializeField, Min(0.01f)] private float flyDuration = 0.42f;
        [SerializeField, Range(0.1f, 1f)] private float flyEndScale = 0.45f;
        [SerializeField, Min(0.01f)] private float vanishDuration = 0.16f;
        [SerializeField, Min(0f)] private float finishDelay = 0.3f;
        [SerializeField, Min(0)] private int starCount = 26;
        [SerializeField, Min(0)] private int sparkleCount = 18;
        [SerializeField, Min(0)] private int puffCount = 16;

        private Coroutine _routine;
        private int _lastPraiseIndex = -1;

        public bool IsPlaying {
            get {
                return _routine != null;
            }
        }

        public void Play(ContainerPiece container, SandSource tank, Action onFinished) {
            if (container == null || tank == null || praisePopup == null) {
                Debug.LogError("[CompletionSequencer] Missing container, tank or praise popup; skipping the sequence.", this);
                onFinished?.Invoke();
                return;
            }

            Stop();
            _routine = StartCoroutine(PlayRoutine(container, tank, onFinished));
        }

        public void Stop() {
            if (_routine != null) {
                StopCoroutine(_routine);
                _routine = null;
            }

            if (praisePopup != null) {
                praisePopup.Hide();
            }

            ClearParticles(starBurst);
            ClearParticles(sparkleBurst);
            ClearParticles(puffBurst);
        }

        private IEnumerator PlayRoutine(ContainerPiece container, SandSource tank, Action onFinished) {
            ContainerVisuals visuals = container.Visuals;
            Transform root = visuals.VisualRoot;
            SpriteRenderer flash = visuals.FlashRenderer;
            Color bodyColor = visuals.BodyColor;
            visuals.LockForAnimation();

            Vector3 center = root.position;
            EmitBurst(starBurst, center, starCount, bodyColor);
            EmitBurst(sparkleBurst, center, sparkleCount, Color.white);
            praisePopup.Show(NextPraiseWord(), center + praiseOffset, praiseColor);

            float elapsed = 0f;
            while (elapsed < popDuration) {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / popDuration);
                root.localScale = new Vector3(popScaleX.Evaluate(t), popScaleY.Evaluate(t), 1f);
                SetAlpha(flash, flashAlpha.Evaluate(t));
                yield return null;
            }

            root.localScale = Vector3.one;
            SetAlpha(flash, 0f);
            yield return Wait(flyDelay);

            Vector3 start = root.position;
            Rect tankRect = tank.InteriorWorldRect;
            Vector3 target = new Vector3(tankRect.center.x, tankRect.center.y, start.z);
            elapsed = 0f;
            while (elapsed < flyDuration) {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / flyDuration);
                root.position = Vector3.LerpUnclamped(start, target, Easing.InBack(t));
                float scale = Mathf.Lerp(1f, flyEndScale, Easing.OutQuad(t));
                root.localScale = new Vector3(scale, scale, 1f);
                yield return null;
            }

            elapsed = 0f;
            while (elapsed < vanishDuration) {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / vanishDuration);
                float scale = flyEndScale * (1f - Easing.InBack(t));
                root.localScale = new Vector3(scale, scale, 1f);
                yield return null;
            }

            root.gameObject.SetActive(false);
            EmitBurst(puffBurst, target, puffCount, bodyColor);
            EmitBurst(starBurst, target, starCount / 2, Color.white);
            yield return Wait(finishDelay);

            _routine = null;
            onFinished?.Invoke();
        }

        private string NextPraiseWord() {
            if (praiseWords == null || praiseWords.Length == 0) {
                return "GREAT!";
            }

            int index = UnityEngine.Random.Range(0, praiseWords.Length);
            if (praiseWords.Length > 1 && index == _lastPraiseIndex) {
                index = (index + 1) % praiseWords.Length;
            }

            _lastPraiseIndex = index;
            return praiseWords[index];
        }

        private static IEnumerator Wait(float seconds) {
            float elapsed = 0f;
            while (elapsed < seconds) {
                elapsed += Time.deltaTime;
                yield return null;
            }
        }

        private static void EmitBurst(ParticleSystem system, Vector3 position, int count, Color color) {
            if (system == null || count <= 0) {
                return;
            }

            ParticleSystem.EmitParams emitParams = new ParticleSystem.EmitParams();
            emitParams.position = position;
            emitParams.applyShapeToPosition = true;
            emitParams.startColor = color;
            system.Emit(emitParams, count);
        }

        private static void ClearParticles(ParticleSystem system) {
            if (system != null) {
                system.Clear(true);
            }
        }

        private static void SetAlpha(SpriteRenderer spriteRenderer, float alpha) {
            Color color = spriteRenderer.color;
            color.a = alpha;
            spriteRenderer.color = color;
        }
    }
}