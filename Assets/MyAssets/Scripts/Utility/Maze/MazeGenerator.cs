using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

namespace Assets.MyAssets.Scripts.Utility.Maze
{
    public class MazeGenerator : MonoBehaviour
    {
        [Header("Gird")]
        [SerializeField] private int    _cols = 20; // 가로
        [SerializeField] private int    _rows = 20; // 세로
        [SerializeField] private float  _cellSize = 5f;

        [Header("Walls")]
        [SerializeField] private GameObject _wallPrefab;
        [SerializeField] private float      _wallThickness = 0.3f;
        [SerializeField] private float      _wallHeight    = 3f;

        [Header("Random Seed")]
        [SerializeField] private int _seed = -1;

        [Header("Layer Assignment")]
        [SerializeField] private string _wallLayerName = "Wall";

        /// <summary>
        /// 벽 오브젝트와 그 벽들이 갖고 있는 컴포넌트(Renderer, Collider)의 참조를 묶은 Struct
        /// </summary>
        private struct WallEntry
        {
            public GameObject gameObject;
            public Renderer   renderer;
            public Collider   collider;
        }

        private Vector2 worldStart;

        private Cell[,] _grid;
        private List<Cell> _allCells = new List<Cell>();

        private readonly List<WallEntry> _wallEntries = new List<WallEntry>();
        private ObjectPool<GameObject>   _wallPool; // 재사용 대기 중인(비활성화된) 벽 오브젝트 풀

        private Transform _wallParent; // 벽 오브젝트들의 공통 부모, Generate()마다 새로 만들지 않고 재사용

        private readonly List<Cell>  _neighborCache = new List<Cell>(4);
        private readonly Stack<Cell> _dfsStack      = new Stack<Cell>();

        public IReadOnlyList<Cell> AllCells => _allCells;
        public int Cols  => _cols;
        public int Rows  => _rows;
        public float CellSize => _cellSize;

        private void Awake()
        {
            worldStart = new Vector2(-_cols * _cellSize * 0.5f, -_rows * _cellSize * 0.5f);

            GameObject wallParentObj = new GameObject("Walls");
            wallParentObj.transform.SetParent(transform);
            _wallParent = wallParentObj.transform;

            _wallPool = new ObjectPool<GameObject>(
                createFunc: CreateWall,
                actionOnGet: wall => wall.SetActive(true),
                actionOnRelease: wall => wall.SetActive(false),
                actionOnDestroy: wall => Destroy(wall),
                collectionCheck: true,
                defaultCapacity: _cols * _rows * 2,
                maxSize: _cols * _rows * 4);
        }

        /// <summary>
        /// 풀이 비어 새로 생성해야 할 때 호출되는 벽 생성 함수
        /// </summary>
        private GameObject CreateWall()
        {
            return Instantiate(_wallPrefab, _wallParent);
        }

        /// <summary>
        /// 미로 사이즈 설정 메소드
        /// </summary>
        public void SetSize(int cols, int rows)
        {
            _cols = cols;
            _rows = rows;

            worldStart = new Vector2(-cols * _cellSize * 0.5f, -rows * _cellSize * 0.5f);
        }

        /// <summary>
        /// 시드값을 직접 지정해 미로를 생성하는 메소드
        /// </summary>
        public void SetSeed(int seed)
        {
            _seed = seed;
        }

        /// <summary>
        /// 미로 생성 메소드
        /// </summary>
        public void Generate()
        {
            ClearWalls();
            InitGrid();
            RunDFS();
            SpawnWalls();
        }

        /// <summary>
        /// 생성된 벽들을 파괴하지 않고 풀에 반납하는 메소드
        /// </summary>
        public void ClearWalls()
        {
            foreach (var entry in _wallEntries)
            {
                if (entry.gameObject != null)
                {
                    _wallPool.Release(entry.gameObject);
                }
            }

            _wallEntries.Clear();
        }

        /// <summary>
        /// 셀을 생성 후 각각의 센터를 구한 후 리스트에 저장
        /// </summary>
        private void InitGrid()
        {
            _allCells.Clear();

            _grid = new Cell[_cols, _rows];

            for (int r = 0; r < _rows; r++)
            {
                for (int c = 0; c < _cols; c++)
                {
                    Vector3 center = new Vector3(worldStart.x + c * _cellSize + _cellSize * 0.5f, 0f, worldStart.y + r * _cellSize + _cellSize * 0.5f);

                    _grid[c, r] = new Cell(c, r, center);
                    _allCells.Add(_grid[c, r]);
                }
            }
        }

        /// <summary>
        /// 깊이 기반(DFS) 벽 생성할 위치 탐색
        /// </summary>
        private void RunDFS()
        {
            // 매번 다른 맵이 나오도록 시드값 설정
            if (_seed == -1)
            {
                Random.InitState(System.DateTime.Now.Millisecond);
            }
            else
            {
                Random.InitState(_seed);
            }

            _dfsStack.Clear();

            Cell startCell = _grid[Random.Range(0, _cols), Random.Range(0, _rows)];
            startCell.visited = true;
            _dfsStack.Push(startCell);

            while (_dfsStack.Count > 0)
            {
                Cell current = _dfsStack.Peek();
                List<Cell> neighbors = GetUnvisitedNeighbors(current);

                if (neighbors.Count > 0)
                {
                    // 이웃한 셀
                    Cell next = neighbors[Random.Range(0, neighbors.Count)];

                    RemoveWallBetween(current, next);

                    next.visited = true;
                    _dfsStack.Push(next);
                }
                else
                {
                    // 막힌 곳
                    _dfsStack.Pop();
                }
            }
        }

        /// <summary>
        /// 범위 내(Grid)에서 현재 셀 주위에 이웃한 미방문 셀 검사
        /// </summary>
        private List<Cell> GetUnvisitedNeighbors(Cell cell)
        {
            _neighborCache.Clear();

            int c = cell.col;
            int r = cell.row;

            if (r + 1 < _rows && !_grid[c, r + 1].visited) _neighborCache.Add(_grid[c, r + 1]); // north
            if (r - 1 >= 0    && !_grid[c, r - 1].visited) _neighborCache.Add(_grid[c, r - 1]); // south
            if (c + 1 < _cols && !_grid[c + 1, r].visited) _neighborCache.Add(_grid[c + 1, r]); // east
            if (c - 1 >= 0    && !_grid[c - 1, r].visited) _neighborCache.Add(_grid[c - 1, r]); // west

            return _neighborCache;
        }

        /// <summary>
        /// 두 셀 사이의 벽을 제거하는 메소드
        /// </summary>
        private void RemoveWallBetween(Cell a, Cell b)
        {
            // dc, dr로 두 셀의 상대적 위치를 파악해 어느 방향 벽을 제거할지 결정
            int dc = b.col - a.col;
            int dr = b.row - a.row;

            // 두 셀은 벽을 공유하므로 양쪽 셀 모두 해당 방향 벽을 제거
            if      (dr == 1)   { a.northWall = false; b.southWall = false; }
            else if (dr == -1)  { a.southWall = false; b.northWall = false; }
            else if (dc == 1)   { a.eastWall  = false; b.westWall  = false; }
            else if (dc == -1)  { a.westWall  = false; b.eastWall  = false; }
        }

        /// <summary>
        /// 벽들을 정해진 위치에 생성하는 메소드
        /// </summary>
        private void SpawnWalls()
        {
            if (_wallPrefab == null) return;

            int wallLayer = LayerMask.NameToLayer(_wallLayerName);

            for (int r = 0; r < _rows; r++)
            {
                for (int c = 0; c < _cols; c++)
                {
                    Cell cell = _grid[c, r];

                    // 셀 중심 월드 좌표
                    float cx = cell.worldCenter.x;
                    float cz = cell.worldCenter.z;

                    if (cell.northWall)
                    {
                        Vector3 pos = new Vector3(cx, _wallHeight * 0.5f, cz + _cellSize * 0.5f);
                        SpawnWall(pos, false, wallLayer);
                    }

                    if (cell.eastWall)
                    {
                        Vector3 pos = new Vector3(cx + _cellSize * 0.5f, _wallHeight * 0.5f, cz);
                        SpawnWall(pos, true, wallLayer);
                    }
                    if (r == 0 && cell.southWall)
                    {
                        Vector3 pos = new Vector3(cx, _wallHeight * 0.5f, cz - _cellSize * 0.5f);
                        SpawnWall(pos, false, wallLayer);
                    }
                    if (c == 0 && cell.westWall)
                    {
                        Vector3 pos = new Vector3(cx - _cellSize * 0.5f, _wallHeight * 0.5f, cz);
                        SpawnWall(pos, true, wallLayer);
                    }
                }
            }
        }

        /// <summary>
        /// 벽 하나 생성(풀에서 재사용), isVertical -> true = Z축 방향 벽, false = X축 방향 벽
        /// </summary>
        private void SpawnWall(Vector3 position, bool isVertical, int wallLayer)
        {
            GameObject wall = _wallPool.Get();

            wall.transform.SetPositionAndRotation(position, Quaternion.identity);
            wall.transform.localScale = isVertical ?
                    new Vector3(_wallThickness, _wallHeight, _cellSize) : 
                    new Vector3(_cellSize, _wallHeight, _wallThickness);

            if (wallLayer != -1)
            {
                wall.layer = wallLayer;
            }

            WallEntry entry = new WallEntry
            {
                gameObject = wall,
                renderer = wall.GetComponent<Renderer>(),
                collider = wall.GetComponent<Collider>()
            };

            // 풀에서 재사용된 벽은 이전 레이어에서 비활성(트리거) 상태였던 이력이 남아있을 수 있음.
            // NavMesh Bake는 트리거 콜라이더를 장애물로 잡지 않으므로, Bake 전에 반드시 솔리드 상태로 리셋해둬야
            // 나중에 SetActiveLayer()가 다시 트리거로 내려도 이미 구워진 NavMesh에 구멍이 남지 않음
            if (entry.renderer != null) entry.renderer.enabled = true;
            if (entry.collider != null) entry.collider.isTrigger = false;

            _wallEntries.Add(entry);
        }

        /// <summary>
        /// 월드 좌표를 셀 위치로 변환하는 메소드
        /// </summary>
        public Vector2Int WorldToCell(Vector3 worldPos)
        {
            int col = (int)((worldPos.x - worldStart.x) / _cellSize);
            int row = (int)((worldPos.z - worldStart.y) / _cellSize);

            col = Mathf.Clamp(col, 0, _cols - 1);
            row = Mathf.Clamp(row, 0, _rows - 1);

            return new Vector2Int(col, row);
        }

        /// <summary>
        /// 셀 위치를 월드 좌표로 변환하는 메소드
        /// </summary>
        public Vector3 CellToWorld(Vector2Int cell)
        {
            return _grid[cell.x, cell.y].worldCenter;
        }

        /// <summary>
        /// [col, row] 위치에 있는 셀을 반환하는 메소드
        /// </summary>
        public Cell GetCell(int col, int row)
        {
            if (col < 0 || col >= _cols || row < 0 || row >= _rows) return null;

            return _grid[col, row];
        }

        /// <summary>
        /// 전체 셀 중 두 셀(주로 시작/목표 지점)을 제외한 나머지를 Fisher-Yates로 섞어 반환하는 메소드.
        /// 스포너들이 각자 독립적으로 호출하므로, 호출할 때마다 새로 섞인 리스트를 반환함
        /// </summary>
        public List<Cell> GetShuffledCandidateCells(Vector2Int excludeA, Vector2Int excludeB)
        {
            List<Cell> candidates = new List<Cell>();

            foreach (Cell cell in _allCells)
            {
                if (cell.col == excludeA.x && cell.row == excludeA.y) continue;
                if (cell.col == excludeB.x && cell.row == excludeB.y) continue;

                candidates.Add(cell);
            }

            for (int i = candidates.Count - 1; i > 0; i--)
            {
                int j = Random.Range(0, i + 1);
                (candidates[i], candidates[j]) = (candidates[j], candidates[i]);
            }

            return candidates;
        }

        /// <summary>
        /// 미로의 모든 벽을 활성/비활성 레이어 상태로 전환
        /// 활성 시: 보이고 충돌함 / 비활성 시: 안 보이고 통과 가능 (콜라이더는 위치 검사를 위해 트리거로 유지)
        /// </summary>
        public void SetWallsActiveState(bool isActiveLayer)
        {
            for (int i = 0; i < _wallEntries.Count; i++)
            {
                WallEntry entry = _wallEntries[i];

                if(entry.renderer != null)
                {
                    entry.renderer.enabled = isActiveLayer;
                }

                if(entry.collider != null)
                {
                    entry.collider.isTrigger = !isActiveLayer;
                }
            }
        }

        /// <summary>
        /// 에디터에서 그리드 확인용
        /// </summary>
        private void OnDrawGizmos()
        {
            // 월드 범위 테두리
            Gizmos.color = Color.green;
            float totalW = _cols * _cellSize;
            float totalH = _rows * _cellSize;
            Vector3 center = new Vector3(worldStart.x + totalW * 0.5f, 0, worldStart.y + totalH * 0.5f);
            Gizmos.DrawWireCube(center, new Vector3(totalW, 0.1f, totalH));

            // 셀 그리드
            Gizmos.color = new Color(1, 1, 0, 0.2f);
            for (int r = 0; r < _rows; r++)
            {
                for (int c = 0; c < _cols; c++)
                {
                    float cx = worldStart.x + c * _cellSize + _cellSize * 0.5f;
                    float cz = worldStart.y + r * _cellSize + _cellSize * 0.5f;
                    Gizmos.DrawWireCube(new Vector3(cx, 0, cz), new Vector3(_cellSize - 0.1f, 0.1f, _cellSize - 0.1f));
                }
            }
        }
    }
}
