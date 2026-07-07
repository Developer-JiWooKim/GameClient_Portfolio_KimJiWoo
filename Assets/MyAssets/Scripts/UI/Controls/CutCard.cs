using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace Assets.MyAssets.Scripts.UI.Controls
{
    [UxmlElement]
    public partial class CutCard : VisualElement
    {
        private static readonly CustomStyleProperty<Color> s_FillProperty = new CustomStyleProperty<Color>("--card-fill");
        private static readonly CustomStyleProperty<Color> s_BorderProperty = new CustomStyleProperty<Color>("--card-border");

        private readonly Clickable _clickable;

        private Color _fillColor = new Color32(0x1A, 0x0D, 0x28, 0xFF);
        private Color _borderColor = new Color32(0x3D, 0x28, 0x60, 0xFF);

        [UxmlAttribute] public float CutSize { get; set; } = 12f;

        public event Action clicked;

        public CutCard()
        {
            focusable = true;
            pickingMode = PickingMode.Position;

            _clickable = new Clickable(() => clicked?.Invoke());
            this.AddManipulator(_clickable);

            generateVisualContent += OnGenerateVisualContent;
            RegisterCallback<CustomStyleResolvedEvent>(OnCustomStyleResolved);
        }

        private void OnCustomStyleResolved(CustomStyleResolvedEvent evt)
        {
            bool changed = false;

            if (evt.customStyle.TryGetValue(s_FillProperty, out Color fill))
            {
                _fillColor = fill;
                changed = true;
            }

            if (evt.customStyle.TryGetValue(s_BorderProperty, out Color border))
            {
                _borderColor = border;
                changed = true;
            }

            if (changed) MarkDirtyRepaint();
        }

        private void OnGenerateVisualContent(MeshGenerationContext mgc)
        {
            Rect rect = contentRect;
            if (rect.width < 1f || rect.height < 1f) return;

            float cut = Mathf.Min(CutSize, Mathf.Min(rect.width, rect.height) * 0.4f);
            float w = rect.width;
            float h = rect.height;

            Painter2D painter = mgc.painter2D;
            painter.BeginPath();
            painter.MoveTo(new Vector2(0, 0));
            painter.LineTo(new Vector2(w - cut, 0));
            painter.LineTo(new Vector2(w, cut));
            painter.LineTo(new Vector2(w, h));
            painter.LineTo(new Vector2(cut, h));
            painter.LineTo(new Vector2(0, h - cut));
            painter.ClosePath();

            painter.fillColor = _fillColor;
            painter.Fill();

            painter.strokeColor = _borderColor;
            painter.lineWidth = 1f;
            painter.Stroke();
        }
    }
}
