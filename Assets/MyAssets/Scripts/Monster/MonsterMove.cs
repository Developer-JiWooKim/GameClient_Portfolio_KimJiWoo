using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class MonsterMove : MonoBehaviour
{
    [Header("Setting")]
    [SerializeField] private float _patrolSpeed    = 4f;   
    [SerializeField] private float _chaseSpeed     = 8f;   
    [SerializeField] private float _rotateSpeed    = 240f;
    [SerializeField] private float _arriveDistance = 0.1f; // 목표 지점 도착 판정 거리

    [Header("Hard 난이도")]
    [Tooltip("Hard 난이도 + Arcane 레이어에서 추격 속도에 곱해지는 배율. 플레이어 이동속도(10)보다 빨라지도록 설정")]
    [SerializeField] private float _hardArcaneChaseSpeedMultiplier = 1.3f;

    private NavMeshAgent _agent;
    private Vector3      _patrolTarget;
    private Vector2Int   _previousCell;
    private bool         _hasPatrolTarget;
    private bool         _hasPreviousCell;

    private List<Vector2Int> _openNeighbors = new List<Vector2Int>(4);

    private void Awake()
    {
        _agent                = GetComponent<NavMeshAgent>();
        _agent.speed          = _patrolSpeed; // 시작은 Patrol 속도로 초기화
        _agent.updateRotation = true; 
    }

    /// <summary>
    /// Idle 상태일 때 미로 안의 랜덤 지점(근처 셀의 Center)을 목표로 순찰하는 메소드
    /// </summary>
    public void Patrol()
    {
        _agent.speed     = _patrolSpeed;
        _agent.isStopped = false;

        if (!_hasPatrolTarget || _agent.remainingDistance <= _arriveDistance)
        {            
            if(TryGetRandomPatrolPoint(out Vector3 point))
            {
                _patrolTarget = point;
                _agent.SetDestination(_patrolTarget);
                _hasPatrolTarget = true;
            }
        }
    }

    /// <summary>
    /// 타겟 위치로 추격 이동 메소드, 경로 탐색과 이동은 NavMeshAgent가 자체적으로 처리
    /// </summary>
    public void MoveToTarget(Vector3 targetPos)
    {
        bool isHardArcane = GameManager.IsHardArcaneMode();

        _agent.speed = isHardArcane ? _chaseSpeed * _hardArcaneChaseSpeedMultiplier : _chaseSpeed;
        _agent.isStopped = false;        
        _agent.SetDestination(targetPos);
    }

    public void StopMovement()
    {
        _agent.isStopped = true;
        _agent.velocity = Vector3.zero;
    }

    /// <summary>
    /// 타겟 방향으로 회전, 공격 범위 안에 있을 때 플레이어를 바라보게 하는 메소드
    /// </summary>
    public void LookAtTarget(Vector3 targetPos)
    {
        Vector3 dir = targetPos - transform.position;
        dir.y = 0f;

        if (dir.sqrMagnitude < 0.001f) return;

        RotateToward(dir);
    }

    /// <summary>
    /// Idle 진입 시 순찰 목표 초기화
    /// </summary>
    public void ClearPath()
    {
        _hasPatrolTarget = false;
        _agent.isStopped = false;
    }

    /// <summary>
    /// 오브젝트 풀에서 재사용될 때 NavMeshAgent를 새 위치로 순간이동시키는 메소드
    /// transform.position을 직접 옮기면 에이전트 내부 상태와 어긋나므로 Warp를 사용
    /// </summary>
    public void Warp(Vector3 position)
    {
        _hasPreviousCell = false;
        ClearPath();
        _agent.Warp(position);
    }

    /// <summary>
    /// 현재 셀 기준으로 벽이 없는 인접 셀 중 하나를 골라 순찰 목표로 반환하는 메소드
    /// 방금 왔던 셀은 막다른 길이 아닌 이상 후보에서 제외 -> 핑퐁 이동 방지
    /// </summary>
    private bool TryGetRandomPatrolPoint(out Vector3 result)
    {
        MazeGenerator mazeGenenrator = MazeLayerManager.Instance.GetActiveMaze();

        Vector3 myPos = transform.position;

        Vector2Int currentCellPos = mazeGenenrator.WorldToCell(myPos);

        Cell currentCell = mazeGenenrator.GetCell(currentCellPos.x, currentCellPos.y);

        if (currentCell == null)
        {
            Debug.LogError("TryGetRandomPatrolPoint currentCell is null");
            result = myPos;
            return false;
        }

        _openNeighbors.Clear();

        if (!currentCell.northWall) _openNeighbors.Add(new Vector2Int(currentCellPos.x, currentCellPos.y + 1));
        if (!currentCell.southWall) _openNeighbors.Add(new Vector2Int(currentCellPos.x, currentCellPos.y - 1));
        if (!currentCell.eastWall)  _openNeighbors.Add(new Vector2Int(currentCellPos.x + 1, currentCellPos.y));
        if (!currentCell.westWall)  _openNeighbors.Add(new Vector2Int(currentCellPos.x - 1, currentCellPos.y));

        if(_hasPreviousCell && _openNeighbors.Count > 1)
        {
            _openNeighbors.Remove(_previousCell);
        }

        if(_openNeighbors.Count == 0)
        {
            result = myPos;
            return false;
        }

        Vector2Int choiceCell = _openNeighbors[Random.Range(0, _openNeighbors.Count)];

        Cell neighborCell = mazeGenenrator.GetCell(choiceCell.x, choiceCell.y);

        _previousCell = currentCellPos;
        _hasPreviousCell = true;

        result = neighborCell.worldCenter;
        result.y = myPos.y;

        return true;
    }

    /// <summary>
    ///  dir 방향으로 고정 각속도(도/초)로 회전 - NavMeshAgent의 Angular Speed와 동일한 모델
    /// </summary>
    private void RotateToward(Vector3 dir)
    {
        Quaternion targetRotation = Quaternion.LookRotation(dir, Vector3.up);

        transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, _rotateSpeed * Time.deltaTime);
    }
}
