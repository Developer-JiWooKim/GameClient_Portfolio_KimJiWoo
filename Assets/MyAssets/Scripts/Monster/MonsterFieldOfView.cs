using UnityEngine;
using Assets.MyAssets.Scripts.Utility.SingleTon;

namespace Assets.MyAssets.Scripts.Monster
{
    [RequireComponent(typeof(MeshFilter))]
    [RequireComponent(typeof(MeshRenderer))]
    public class MonsterFieldOfView : MonoBehaviour
    {
        [Header("Mesh Quality")]
        [Tooltip("Ray 개수가 많을수록 부드럽지만 연산 비용 증가")]
        [SerializeField] private int _rayCount = 90;

        [Header("Update Rate")]
        [Tooltip("이 시간(초)마다 한 번씩만 메시를 다시 계산 - 매 프레임 다시 그리면 몬스터 수가 늘수록 비용이 커짐")]
        [SerializeField] private float _updateInterval = 0.05f;

        [Header("Rendering")]
        [SerializeField] private float    _meshHeight = 0.1f;
        [SerializeField] private Material _fovMaterial;

        private float _updateTimer;

        private float _detectionRange = 0f;
        private float _fieldOfView    = 0f;

        private Mesh       _mesh;
        private MeshFilter _meshFilter;

        private Vector3[] _vertices;
        private int[]     _triangles;

        private void Awake() => Initialize();

        /// <summary>
        /// 초기화 메소드
        /// </summary>
        private void Initialize()
        {
            _meshFilter = GetComponent<MeshFilter>();

            _mesh = new Mesh { name = "FieldOfViewMesh" };
            _mesh.MarkDynamic(); // GC 생성을 방지하기 위해 동적 Mesh임을 미리 선언
            _meshFilter.mesh = _mesh;

            _vertices  = new Vector3[_rayCount + 2];
            _triangles = new int[_rayCount * 3];
            for (int i = 0; i < _rayCount; i++)
            {
                _triangles[i * 3] = 0;
                _triangles[i * 3 + 1] = i + 1;
                _triangles[i * 3 + 2] = i + 2;
            }

            GetComponent<MeshRenderer>().material = _fovMaterial;

            if (transform.parent.TryGetComponent(out MonsterSight sight))
            {
                _detectionRange = sight.DetectionRange;
                _fieldOfView = sight.FieldOfView;
            }
            else
            {
                Debug.LogError("MonsterFieldOfView Initialize(): The parent object doesn't have a MonsterSight component.");
            }

            // 여러 몬스터가 같은 프레임에 한꺼번에 재계산하지 않도록 시작 타이밍을 랜덤하게 어긋나게
            _updateTimer = Random.Range(0f, _updateInterval);
        }

        /// <summary>
        /// 매 프레임 시야각 메시를 갱신하는 메소드
        /// </summary>
        public void DrawFieldOfView(Transform monsterTransform)
        {
            _updateTimer -= Time.deltaTime;
            if (_updateTimer > 0f) return;
            _updateTimer = _updateInterval;

            float angleStep   = _fieldOfView / _rayCount;
            float startAngle  = -_fieldOfView * 0.5f;
            float originAngle = monsterTransform.eulerAngles.y;

            Vector3 originPos = monsterTransform.position;
            originPos.y = _meshHeight;

            _vertices[0] = Vector3.zero;

            if(MazeLayerManager.Instance == null)
            {
                Debug.LogError("MazeLayerManager Instance is null");
            }

            int wallLayerMask = MazeLayerManager.Instance != null ? MazeLayerManager.Instance.CurrentWallLayerMask : 0;

            for (int i = 0; i <= _rayCount; i++)
            {
                float angle = originAngle + startAngle + angleStep * i;
                float rad = angle * Mathf.Deg2Rad;

                Vector3 direction = new Vector3(Mathf.Sin(rad), 0f, Mathf.Cos(rad));
                Vector3 endPoint;

                if (Physics.Raycast(originPos, direction, out RaycastHit hit, _detectionRange, wallLayerMask))
                {
                    endPoint = hit.point;
                }
                else
                {
                    endPoint = originPos + direction * _detectionRange;
                }

                endPoint.y = _meshHeight;

                // monsterTransform 기준으로 로컬 변환(이 스크립트가 붙어있는 FieldOfView가 몬스터 프리팹의 자식으로 붙어있기 때문)
                _vertices[i + 1]   = monsterTransform.InverseTransformPoint(endPoint);
                _vertices[i + 1].y = 0f;
            }

            _mesh.Clear();
            _mesh.SetVertices(_vertices);
            _mesh.SetTriangles(_triangles, 0);
            _mesh.RecalculateBounds();
        }
    }
}
