namespace Assets.MyAssets.Scripts.Utility.Maze
{
    public class Cell
    {
        public readonly int col;
        public readonly int row;

        // 셀 위치
        public UnityEngine.Vector2Int GridPos => new UnityEngine.Vector2Int(col, row);

        // 셀 중앙
        public UnityEngine.Vector3 worldCenter;

        public bool visited;

        // 각 방향 벽 (true = 벽 있음)
        public bool northWall = true;
        public bool southWall = true;
        public bool eastWall = true;
        public bool westWall = true;

        public Cell(int col, int row, UnityEngine.Vector3 worldCenter)
        {
            this.col = col;
            this.row = row;
            this.worldCenter = worldCenter;
        }
    }
}
