# 게임 포트폴리오 과제

## 프로젝트 개요

플레이어는 꿈의 미로에 갇혀 있습니다. 미로 곳곳에 흩어진 **꿈의 조각(열쇠)** 를 모두 모으면 미로 오른쪽 위 구석에 **Goal Point**가 생성되고, 그곳에 도달하면 탈출(클리어)합니다. 

이 미로는 `Tab` 커맨드로 **두 개의 레이어(미로)를 전환**할 수 있으며, 각 레이어는 서로 다른 미로 구조를 가집니다.

| 레이어 | 특징 |
|---|---|
| **Physical** | 기본 레이어. 정신력이 회복됨 |
| **Arcane** | 다른 구조의 미로. 머무는 동안 정신력이 지속 소모됨 |

### 게임 오버 조건

- 미로를 배회하는 **몬스터(도플갱어)** 에게 발각되면 추격당하고, 공격 범위 안에 들어가면 피격됩니다. **3회 피격되어 HP가 0**이 되면 게임 오버
- **Arcane 레이어**에 오래 머물러 **정신력이 0**이 되면 게임 오버

### 난이도

| 난이도 | 차이점 |
|---|---|
| **Normal** | 정신력 소모 4/초, 회복 10/초 |
| **Hard** | 정신력 소모 8/초, 회복 4/초 + Arcane 레이어에서 몬스터가 플레이어를 항시 감지하고 추격 속도 증가 |

### 실행 환경

| 항목 | 내용 |
|---|---|
| 유니티 버전 | Unity 6 (6000.4.9f1) |
| 렌더 파이프라인 | URP |
| 장르 | 미로 탈출 / 추격 회피 |
| 메인 씬 | `Assets/MyAssets/Scenes/GameClientAssignment.unity` |
| 권장 해상도 | Full HD (1920×1080) |

---

## 시연 방법

1. Unity 6(6000.4.9f1, URP)로 프로젝트 열기
2. `Assets/MyAssets/Scenes/GameClientAssignment.unity` 씬 열고 **Play** (Game 뷰를 Full HD로 설정)
3. **Title 화면** → `Start` 버튼 클릭
4. **Select 화면** → 난이도(`Normal` / `Hard`) 선택
5. **Intro 연출** — 미로 생성 → 플레이어 스폰 → Intro 카메라 → 플레이어 등장 애니메이션 → Quarter View 카메라로 블렌드
6. **InGame HUD**(HP / 열쇠 / 정신력 / 타이머)가 표시되면 조작 시작

**※ 시연 포인트**
   1. `WASD` 이동 → 카메라가 따라오는 것 확인
   2. 열쇠에 접근 → 획득 효과음 + HUD 열쇠 카운트 증가
   3. `Tab` 입력 → 화면 일렁임 + 미로 구조/조명 전환 + 정신력 게이지 감소 시작
   4. 벽에 붙어서 `Tab` → 전환 실패 피드백(보라색 플래시 + 카메라 흔들림 + 경고 UI)
   5. 몬스터에게 접근 → 발각 → 추격 → 피격(화면 붉은 페이드 + 카메라 흔들림 + HP 감소)
   6. `Esc` → Pause 패널 → 다시 `Esc`로 재개(인게임 플레이 중에만 가능)
   7. 열쇠 5개 수집 → Goal Point 생성 → 도달 → **Result 화면(CLEAR!!)**

---

### 조작법

| 입력 | Input Action | 기능 |
|---|---|---|
| `W` `A` `S` `D` / 방향키 | `Move` (Value, Vector2) | 상하좌우 이동 |
| `Tab` | `SwitchLayer` (Button) | 미로 레이어 전환 (Physical ↔ Arcane) |
| `Esc` | `Pause` (Button) | 일시정지 / 재개 토글 (게임 플레이 중에만 동작) |
| 마우스 좌클릭 | `OnClickEvent` | UI 버튼 조작 |

---

## 실행 화면

### 게임 실행
<img src="Assets/MyAssets/Screenshots/Title.jpg" width="800">

### 게임 플레이 
<img src="Assets/MyAssets/Screenshots/Play.jpg" width="800">

### 게임 클리어
<img src="Assets/MyAssets/Screenshots/Clear.jpg" width="800">

---

## 주요 기능

| # | 필수 구현 | 구현 내용 | 주요 파일 |
|---|---|---|---|
| 1 | Input System 기반 입력 처리 | `InputSystem_Actions` 에셋의 `Player` 액션 맵 + `PlayerInput`(Invoke C# Events) → `onActionTriggered` 이벤트 처리 | `PlayerInputHandler.cs` |
| 2 | 플레이어 조작 | `CharacterController` 기반 이동, 입력값을 월드 방향으로 변환해 이동 + 애니메이션 연동 | `PlayerController.cs`, `PlayerMove.cs` |
| 3 | 상태 변화 | 플레이어(Idle/Move/피격/무적/발각 표정/정신력 고갈), 몬스터 FSM(Idle, Chase, Attack), 게임 흐름 FSM(Title, Select, Playing, Paused, Result) | `MonsterFSM.cs`, `GameFlowFSM.cs`, `PlayerSanity.cs` |
| 4 | Cinemachine 카메라 | `CinemachineBrain` + `CM_IntroCamera`/`CM_QuarterViewCamera` 우선순위 블렌딩, `CinemachineFollow`로 플레이어 추적, Impulse로 피격 흔들림 | `IntroCameraSequencer.cs` |
| 5 | 상호작용 대상 | 열쇠 획득, Goal Point 도달, 몬스터 공격 판정 — 모두 물리 Trigger 기반 | `Key.cs`, `GoalPoint.cs`, `MonsterAttackTrigger.cs` |
| 6 | 게임 흐름 | 열쇠 5개 수집 → Goal Point 생성 → 도달 시 클리어, HP/정신력 소진 시 게임 오버 | `GameRule.cs`, `GoalPointSpawner.cs` |
| 7 | 플레이 루프 | Title → Select(난이도) → Intro 연출 → Playing → (Pause) → Result(Clear or Over) → Replay/Select 재진입 | `GameUIController.cs`, `Scripts/UI/States/` |
| 8 | 시연 가능 상태 | 단일 메인 씬에서 전체 루프 실행 가능, 시연 순서·조작법·캡처 본 문서에 명시 | `README.md` |

---

## 필수 구현 상세

### 1. Input System 기반 입력 처리

**입력 에셋**: `Assets/InputSystem_Actions.inputactions`

- **Action Map**: `Player` (게임 플레이), `UI` (버튼 조작)
- 이 프로젝트에서 실제 사용하는 액션은 `Move`, `SwitchLayer`, `Pause` 3개

| Action | Type | Binding |
|---|---|---|
| `Move` | Value / Vector2 | `WASD` 2D Vector Composite, 방향키 2D Vector Composite |
| `SwitchLayer` | Button | `<Keyboard>/tab` |
| `Pause` | Button | `<Keyboard>/escape` |

**스크립트 연결 구조**

```
InputSystem_Actions
        │
        ▼
PlayerInput 컴포넌트 (Player_CC 프리팹)
   Behavior = Invoke C Sharp Events
        │  onActionTriggered
        ▼
PlayerInputHandler.HandleActionTriggered(context)
        │
        ├─ Move → _inputVector 갱신 ──► PlayerController.Update() → PlayerMove.Move()
        ├─ "SwitchLayer" → OnLayerSwitchRequested 이벤트 ► MazeLayerManager.SwitchLayer()
        └─ "Pause" → OnPauseRequested 이벤트 ──► GameUIController.HandlePauseToggle()
```
- `IsControlEnabled` 프로퍼티로 Pause/Result/Intro 연출 중 이동·레이어 전환 입력을 차단

### 2. 플레이어 조작

- `PlayerController.Update()`가 `PlayerInputHandler.InputVector`(Vector2)를 읽어 `Vector3(x, 0, y)` 방향으로 변환
- `PlayerMove`가 `CharacterController.Move()`로 이동 + 이동 방향으로 회전
- 입력 유무를 `PlayerAnim.SetMoving()`으로 전달해 Idle ↔ Run 애니메이션 전환
- 조작 결과는 화면상 캐릭터 이동, 애니메이션, 카메라 추적, 시야(Fog of War) 확장으로 즉시 확인 가능

### 3. 상태 변화

**플레이어 상태**

| 상태 | 구현 |
|---|---|
| Idle / Move | 입력 유무 → `PlayerAnim.SetMoving()` |
| 등장(Intro) | `PlayerAnim.PlayIntroAsync()` — 애니메이션 종료까지 await |
| 피격 / 무적 | `PlayerController.TakeDamage()` — HP 감소 후 `_invincibleDuration`(1.5초) 동안 재피격 무시 |
| 발각 | `NotifyDetected()` — 감지 중인 몬스터 수를 카운트해 표정 변경(`PlayerFaceController`) |
| 사망 | HP 0 → `OnDead` 이벤트 → `GameRule.GameOver()` |
| 정신력 고갈 | `PlayerSanity` — Arcane 체류 시 감소 / Physical 복귀 시 회복, 0이 되면 `OnSanityDepleted` |

**몬스터 FSM** (`MonsterFSM.cs` + `IMonsterState`)

- `MonsterSight` / `MonsterFieldOfView`가 시야각·거리·차폐(벽 레이어 마스크)를 검사
- `Tick()`은 같은 프레임에 상태가 바뀌면 새 상태까지 이어서 실행(최대 3회, 무한 루프 방지)
- 상태 변경 시 `OnStateChanged` 이벤트로 애니메이션·플레이어 발각 알림 처리
- 오브젝트 풀 재사용 시 `ResetState()`로 Idle 초기화

**게임 흐름 FSM** (`GameFlowFSM.cs` + `IGameFlowState`)

### 4. Cinemachine 카메라 구성

| 구성 요소 | 역할 |
|---|---|
| `Main Camera` + **CinemachineBrain** | 활성 CinemachineCamera를 선택하고 실제 카메라에 반영. Default Blend = **EaseInOut / 1.5초**. 연출 대기 중 `Time.timeScale = 0` 상태에서도 블렌드가 진행되도록 `IgnoreTimeScale = true` 설정 |
| `CM_IntroCamera` (Priority **20**) | 게임 시작 시 캐릭터 정면을 비추는 인트로 시점 |
| `CM_QuarterViewCamera` (Priority **10** → 전환 시 **21**) | **CinemachineFollow** 로 플레이어를 추적, 쿼터뷰 |

**Impulse (카메라 흔들림)**
- `PlayerController`의 `CinemachineImpulseSource` → 피격 시 `GenerateImpulse()`
- 레이어 전환이 벽에 막힐 때도 별도 Impulse Source로 짧은 흔들림
- `CM_QuarterViewCamera`의 `CinemachineImpulseListener`(Gain 1, Use Camera Space)가 수신해 흔들림 적용

### 5. 상호작용 대상

| 대상 | 트리거 방식 | 결과 피드백 |
|---|---|---|
| **열쇠(Key)** | `OnTriggerEnter` + `Player` 태그 검사 | 획득 효과음, HUD 열쇠 카운트 갱신, 오브젝트 풀 반납, 5개 달성 시 `OnAllKeysCollected` 발행 |
| **Goal Point** | `OnTriggerEnter` + `Player` 태그 검사 | 도달 효과음, `GameRule.Clear()` → Result 패널 `CLEAR!!` |
| **몬스터 공격 범위** | `MonsterAttackTrigger`의 Trigger 진입/이탈 | 화면 붉은 페이드(`DamageflashUI`), 카메라 Impulse 흔들림, 피격 효과음, HUD HP 감소 |
| **레이어 전환(벽)** | `Physics.CheckSphere`로 대상 레이어 벽과 겹침 검사 | 성공: 화면 일렁임(Shader Graph Ripple) + 미로/조명/NavMesh 전환 / 실패: 보라색 플래시 + 카메라 흔들림 + 경고 UI |

### 6. 게임 흐름

**클리어 조건 흐름** (`GameRule.cs`)

```
Key.OnTriggerEnter
   └► GameRule.CollectKey()
         ├► OnKeyCollected(current, required) ─► 효과음 + InGamePanel.UpdateKeyCount()
         └► (5/5 달성) OnAllKeysCollected ──► GoalPointSpawner.SpawnGoalPoint() + 생성 효과음
                                                        │
GoalPoint.OnTriggerEnter ──► GameRule.Clear() ──► OnClear ─┴► 타이머 정지 + 클리어 효과음 + Result("CLEAR!!")
```

**게임 오버 흐름**

```
PlayerController.OnDead      ─┐
PlayerSanity.OnSanityDepleted ┼─► GameRule.GameOver() ─► OnGameOver ─► 타이머 정지 + 효과음 + Result("GAME OVER..")
Pause 패널의 End 버튼         ─┘
```

### 7. 플레이 루프

- `GameFlowFSM`이 `TitleState` / `SelectState` / `PlayingState` / `PausedState` / `ResultState`의 `Enter`·`Exit`를 호출해 패널 표시, 타임스케일, 입력 허용 여부를 일괄 관리
- 새 판 시작 진입점(난이도 확정 / Pause의 Replay / Result의 Replay)은 모두 `StartNewGame()` 하나로 수렴
- `RunGameStartSequence()`가 미로 생성 → 유닛 스폰 → 인트로 애니메이션 → 카메라 블렌드까지 `Awaitable`로 대기하고, 그동안 타이머·몬스터·플레이어 입력을 정지시켰다가 한 번에 게임을 시작
- 사용자는 HUD(HP / 열쇠 `n/5` / 정신력 게이지 / 경과 시간)로 목표와 진행도를, Result 패널로 최종 결과와 클리어 타임을 확인 가능

### 8. 시연 / 빌드 가능 상태

- **단일 씬**(`GameClientAssignment.unity`)에서 전체 플레이 루프가 실행

---

## 선택 구현 대응

| 선택 항목 | 구현 여부 | 내용 |
|---|---|---|
| Cinemachine 시점 전환 / 블렌드 | ○ | Intro ↔ Quarter View 우선순위 기반 블렌드(EaseInOut 1.5초), Impulse 카메라 흔들림 |
| 상태 표시 UI | ○ | HP, 열쇠 수집 수, 정신력 게이지, 경과 시간 HUD + 레이어 전환 불가 경고 |
| 이벤트 흐름 | ○ | 열쇠 획득 → Goal Point 생성 → 클리어 판정, 난이도별 규칙 분기 |
| 사운드 / VFX | ○ | `SoundManager` + `SoundLibrary`(SO), Goal Point 파티클, 레이어 전환 화면 일렁임(Shader Graph), 피격 화면 페이드 |
