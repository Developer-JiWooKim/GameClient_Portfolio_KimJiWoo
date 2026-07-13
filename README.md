# 게임 포트폴리오 과제

## 프로젝트 개요

플레이어는 꿈의 미로에 갇혀 있습니다. 미로 곳곳에 흩어진 **꿈의 조각(열쇠)**를 모두 모으면 미로 오른쪽 위 구석에 **Goal Point**가 생성되고, 그곳에 도달하면 탈출(클리어)합니다. 

이 꿈의 미로는 특정 커맨드를 입력하여 전환할 수 있습니다.
- *Physical*
- *Arcane*

게임 오버 조건
- 미로 안에는 몬스터가 돌아다니고 있으며, 플레이어를 발견 시 추격·공격(공격 범위 안에 들어올 시)하며, 3회 공격당해 HP가 0이 되면 게임 오버입니다.

- `Arcane`미로 안에서는 지속적으로 정신력이 소모됩니다. 이 정신력이 0이 되면 게임 오버입니다.

| 항목 | 내용 |
|---|---|
| 유니티 버전 | Unity 6 (6000.4.9f1) |
| 렌더 파이프라인 | URP |
| 장르 | 미로 탈출 / 추격 회피 |
| 메인 씬 | `Assets/MyAssets/Scenes/GameClientAssignment.unity` |

---

## 시연 방법

1. Unity 6(6000.4.9f1, URP)로 프로젝트 열기
2. `Assets/MyAssets/Scenes/GameClientAssignment.unity` 씬 열고 Play(Full HD)
3. 타이틀 화면 Start버튼 클릭 → 난이도(Normal/Hard) 선택 → Intro 연출 후 InGameUI가 나오면 게임 시작

---

### 조작법

| 입력 | 기능 |
|---|---|
| `W` `A` `S` `D` / 방향키 | 상하좌우 이동 |
| `Tab` | 미로 레이어 전환 (Physical ↔ Arcane) |
| `Esc` | 일시정지(게임 플레이 중에만 동작), 게임재개(일시정지 상태에서 한번더 누르면 일시정지 해제) |

---

## 실행 화면

### 게임 실행
<img src="Assets/MyAssets/Screenshots/Title.jpg" width="800">

### 게임 플레이 
<img src="Assets/MyAssets/Screenshots/Play.jpg" width="800">

### 게임 클리어
<img src="Assets/MyAssets/Screenshots/Clear.jpg" width="800">

---

## 사용한 엔진 기능

| 분류 | 적용 내용 |
|---|---|
| **Animation / Animator** | Player/Monster 각각 `Animator` 기반 애니메이션(`PlayerAnim.cs`, `MonsterAnim.cs`) |
| **UI** | UGUI Canvas 하위 Title/Select/InGame/Pause/Result/Help 패널(UIToolKit)을 `GameUIController.cs` + `GameFlowFSM`(상태 패턴)이 조율, `DamageflashUI.cs`로 피격 시 화면 붉은색 페이드 |
| **사운드** | `SoundManager`(싱글톤) + `SoundLibrary`(ScriptableObject, AudioClip 보관) |
| **VFX** | Goal Point 프리팹의 `ParticleSystem`, `ScreenRippleController.cs` + ShaderGraph(`SG_LayerTransitionRipple`) 기반 레이어 전환 화면 일렁임 연출 |
| **물리 Trigger** | 열쇠 회수(`Key.cs`), Goal Point 도달(`GoalPoint.cs`), 몬스터 공격 범위 판정(`MonsterAttackTrigger.cs`) |
| **Cinemachine** | `CinemachineBrain` + `CM_IntroCamera`/`CM_QuarterViewCamera`(우선순위 기반 블렌딩)로 카메라 전환 연출, 피격 시 `Cinemachine Impulse` 카메라 흔들림 |
| **조명 / 머티리얼** | Physical/Arcane 벽 머티리얼(ShaderGraph `SG_PhysicalGrime`/`SG_ArcaneNoise`), Key/Goal Point 발광 머티리얼(`SG_GlowOrb`), 레이어 전환에 맞춘 Directional Light·Fog 색 전환(`LayerLightingController.cs`) |
| **이동 / 경로 시스템** | Player: `CharacterController`(`PlayerMove.cs`) 기반 이동 / Monster: `NavMeshAgent` + `NavMeshSurface`(레이어별 독립 베이크, `MazeLayerManager.cs`) 기반 이동 |

### 추가로 사용한 시스템
- **Input System** — `PlayerInput.onActionTriggered` 이벤트 기반 입력 처리, `PlayerInputHandler.cs`
- **Awaitable 비동기** — 레이어 전환 시퀀스, 카메라 인트로 딜레이 등 시간 기반 연출 처리
- **FSM** — 몬스터 AI(`MonsterFSM.cs` + `IMonsterState.cs` 구현체), UI 흐름(`GameFlowFSM.cs` + `IGameFlowState.cs` 구현체) 양쪽에 State 패턴 적용
- **오브젝트 풀링** — 몬스터/열쇠/벽(Physical/Arcane Wall)은 `UnityEngine.Pool.ObjectPool<GameObject>`로 재사용(`MonsterSpawner.cs`, `KeySpawner.cs`, `MazeGenerator.cs`)

---

## 리소스 구성

```
Assets/
└─ MyAssets/                  # 직접 작업한 리소스
   ├─ Animation/              # 애니메이션 클립들(Player, Monster)
   ├─ Animator/               # Animator Controller(Player, Monster)
   ├─ Fonts/                  # Font
   ├─ Materials/              # 직접 만든 머티리얼들
   ├─ Prefabs/                # Player, Monster, Key, GoalPoint 등
   ├─ Scenes/                 # GameClientAssignment (메인 씬)
   ├─ ScriptableObjectAssets/ # SoundLibrary 에셋
   ├─ Scripts/
   │   ├─ Monster/            # 몬스터 AI(FSM, Sight, Move, Anim)
   │   ├─ Player/             # 플레이어 입력/이동/애니메이션
   │   ├─ ScriptableObject/   # SoundLibrary 등 SO 클래스
   │   ├─ UI/                 # 패널, GameFlowFSM
   │   ├─ Utility/            # Maze, Spawners, SingleTon(Manager), Visuals, Core
   │   └─ Obsolete/           # AStarPathfinder, FollowCamera ([Obsolete] 처리, 참고용)
   ├─ ShaderGraphs/           # SG_PhysicalGrime, SG_ArcaneNoise, SG_GlowOrb, SG_LayerTransitionRipple
   └─ SoundClips/             # AudioClip 원본
```

#### ※ 외부 에셋(`AOSFogWar`, `Hand Painted Stone Texture`, `SD Unity-Chan Haon Custom`, `Toon Shaders Pro`, `unity-chan!`, `TextMesh Pro`)은 원래 임포트 경로를 그대로 유지
