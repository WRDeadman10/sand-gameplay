using System;

using UnityEngine;

namespace SandGameplay.Sand {
    /// <summary>One sand color: a tone ramp (shadow, base, highlight) plus the plastic color of boxes that accept it.</summary>
    [Serializable]
    public struct SandColorDefinition {
        [SerializeField] private string name;
        [SerializeField] private Color shadow;
        [SerializeField] private Color baseColor;
        [SerializeField] private Color highlight;
        [SerializeField] private Color body;

        public SandColorDefinition(string colorName, Color shadowColor, Color midColor, Color highlightColor, Color bodyColor) {
            name = colorName;
            shadow = shadowColor;
            baseColor = midColor;
            highlight = highlightColor;
            body = bodyColor;
        }

        public string Name {
            get {
                return name;
            }
        }

        public Color Shadow {
            get {
                return shadow;
            }
        }

        public Color Base {
            get {
                return baseColor;
            }
        }

        public Color Highlight {
            get {
                return highlight;
            }
        }

        public Color Body {
            get {
                return body;
            }
        }
    }
}