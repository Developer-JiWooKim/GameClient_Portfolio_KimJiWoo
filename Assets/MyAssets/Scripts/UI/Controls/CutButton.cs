using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace Assets.MyAssets.Scripts.UI.Controls
{
    /// <summary>
    /// UI_Design_Reference.md의 비대칭 절단 버튼(clip-path: polygon(8px 0%, 100% 0%, calc(100% - 8px) 100%, 0% 100%))을
    /// USS에 clip-path가 없어 Painter2D로 직접 그리는 커스텀 컨트롤.
    /// Button을 상속하면 내부 텍스트 렌더링과 같은 generateVisualContent 이벤트를 공유해 그림 순서가 꼬이므로,
    /// VisualElement + 자식 Label + Clickable 매니퓰레이터 조합으로 구성한다(자식은 항상 부모 콘텐츠 위에 그려짐).
    /// </summary>
    [UxmlElement]
    public partial class CutButton : VisualElement
    {
        private static readonly CustomStyleProperty<Color> s_FillProperty = new CustomStyleProperty<Color>("--btn-fill");
        private static readonly CustomStyleProperty<Color> s_BorderProperty = new CustomStyleProperty<Color>("--btn-border");

        private readonly Label _label;
        private readonly Clickable _clickable;

        private Color _fillColor = new Color32(0x7B, 0x3F, 0xA0, 0xFF);
        private Color _borderColor = new Color32(0x7B, 0x3F, 0xA0, 0xFF);

        [UxmlAttribute] public float CutSize { get; set; } = 8f;

        [UxmlAttribute]
        public string Text
        {
            get => _label.text;
            set => _label.text = value;
        }

        public event Action clicked;

        public CutButton()
        {
            focusable = true;
            pickingMode = PickingMode.Position;

            _label = new Label { pickingMode = PickingMode.Ignore };
            Add(_label);

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

            float cut = Mathf.Min(CutSize, rect.width * 0.4f);

            Painter2D painter = mgc.painter2D;
            painter.BeginPath();
            painter.MoveTo(new Vector2(cut, 0));
            painter.LineTo(new Vector2(rect.width, 0));
            painter.LineTo(new Vector2(rect.width - cut, rect.height));
            painter.LineTo(new Vector2(0, rect.height));
            painter.ClosePath();

            painter.fillColor = _fillColor;
            painter.Fill();

            painter.strokeColor = _borderColor;
            painter.lineWidth = 1f;
            painter.Stroke();
        }
    }
}
