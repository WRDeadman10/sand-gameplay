using UnityEngine;

namespace SandGameplay.Utilities {
    /// <summary>Standard easing curves for t in [0, 1].</summary>
    public static class Easing {
        private const float BackOvershoot = 1.70158f;

        public static float OutQuad(float t) {
            float inverse = 1f - t;
            return 1f - (inverse * inverse);
        }

        public static float InBack(float t) {
            return ((BackOvershoot + 1f) * t * t * t) - (BackOvershoot * t * t);
        }

        public static float OutBack(float t) {
            float shifted = t - 1f;
            return 1f + ((BackOvershoot + 1f) * shifted * shifted * shifted) + (BackOvershoot * shifted * shifted);
        }

        public static float InOutSine(float t) {
            return -(Mathf.Cos(Mathf.PI * t) - 1f) * 0.5f;
        }
    }
}