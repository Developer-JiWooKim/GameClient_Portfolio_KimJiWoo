using UnityEngine;
using UnityEngine.UIElements;

namespace Assets.MyAssets.Scripts.UI
{
    /// <summary>
    /// 공유 UIDocument 트리 안의 한 화면(named root VisualElement)을 표시/숨김으로 제어하는
    /// UI 패널의 공통 기반 클래스. 모든 화면이 하나의 UIDocument/UXML 아래 형제로 존재하므로
    /// Show/Hide는 GameObject.SetActive 대신 자신의 루트 엘리먼트 display 토글로 구현한다.
    /// </summary>
    public abstract class BasePanelUI : MonoBehaviour
    {
        [SerializeField] protected UIDocument _document;
        [SerializeField] protected string _rootElementName;

        private VisualElement _root;

        /// <summary>
        /// Unity 초기화는 Awake -> OnEnable -> Start 순으로 씬 전체에 걸쳐 단계별로 완료되므로,
        /// UIDocument.rootVisualElement(OnEnable에서 생성)는 어떤 컴포넌트의 Start()에서 접근해도 non-null이 보장된다.
        /// 다만 이 패널 자신의 Start()가 다른 패널(예: GameUIController)의 Start()보다 늦게 실행될 수 있으므로,
        /// 조회 자체는 지연 평가해 Start() 실행 순서에 의존하지 않도록 한다.
        /// </summary>
        protected VisualElement Root => _root ??= _document.rootVisualElement.Q<VisualElement>(_rootElementName);

        protected virtual void Start() { }

        public virtual void Show() => Root.style.display = DisplayStyle.Flex;
        public virtual void Hide() => Root.style.display = DisplayStyle.None;
    }
}
