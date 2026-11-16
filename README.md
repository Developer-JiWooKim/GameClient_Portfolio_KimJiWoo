# 꿈의 미로 - Echoes Of Me

두 개의 미로 레이어(Physical ↔ Arcane)를 오가며 열쇠를 모아 탈출하는 Unity 6(URP) 미로 탈출 게임입니다.

- **포트폴리오 페이지**: https://developer-jiwookim.github.io/GameClient_Portfolio_KimJiWoo/
  (게임 소개, 시연 영상, 설계, 트러블슈팅 정리)
- **플레이 빌드 (Windows)**: [Releases](https://github.com/Developer-JiWooKim/GameClient_Portfolio_KimJiWoo/releases/latest)

> 이 저장소에는 **직접 작성한 C# 스크립트만** 남겨 두었습니다.
> 씬·프리팹·에셋·프로젝트 설정은 포함하지 않으므로 Unity 프로젝트로 열 수 없습니다.
> 실행은 위 빌드를 이용해 주세요.

## 스크립트 구조

원래 프로젝트의 `Assets/MyAssets/Scripts/`를 그대로 루트로 옮겼습니다. 네임스페이스(`Assets.MyAssets.Scripts...`)는 원래 경로 기준입니다.

```
Scripts/
├─ Player/              플레이어 입력·이동·애니메이션·피격·정신력
├─ Monster/             몬스터 시야 판정·이동·공격 판정
│  └─ States/           몬스터 FSM 상태 (Idle / Chase / Attack)
├─ UI/                  UI Toolkit 패널, 게임 흐름 FSM
│  ├─ States/           게임 흐름 상태 (Title / Select / Playing / Paused / Result)
│  └─ Controls/         커스텀 VisualElement 컨트롤
├─ Utility/
│  ├─ Core/             게임 규칙, 난이도, 타이머, Awaitable 유틸
│  ├─ Maze/             미로 생성
│  ├─ SingleTon/        GameManager, MazeLayerManager, SoundManager
│  ├─ Spawners/         플레이어·몬스터·열쇠·Goal Point 스폰 (열쇠·몬스터는 오브젝트 풀)
│  ├─ TriggerEvent/     열쇠 / Goal Point 트리거
│  └─ Visuals/          인트로 카메라, 레이어 전환 연출, 조명, 셰이더 제어
├─ ScriptableObject/    SoundLibrary
└─ Obsolete/            미사용 코드 (NavMesh로 대체한 직접 구현 A*, Cinemachine으로 대체한 카메라)
```

### 먼저 보면 좋은 파일

| 주제 | 파일 |
|---|---|
| 게임 흐름 FSM / 시작 시퀀스 | [`UI/GameFlowFSM.cs`](Scripts/UI/GameFlowFSM.cs), [`UI/GameUIController.cs`](Scripts/UI/GameUIController.cs) |
| 몬스터 FSM | [`Monster/MonsterFSM.cs`](Scripts/Monster/MonsterFSM.cs), [`Monster/States/`](Scripts/Monster/States) |
| 레이어 전환 | [`Utility/SingleTon/MazeLayerManager.cs`](Scripts/Utility/SingleTon/MazeLayerManager.cs), [`Utility/Visuals/LayerTransitionSequencer.cs`](Scripts/Utility/Visuals/LayerTransitionSequencer.cs) |
| 미로 생성 | [`Utility/Maze/MazeGenerator.cs`](Scripts/Utility/Maze/MazeGenerator.cs) |
| 클리어 / 게임 오버 규칙 | [`Utility/Core/GameRule.cs`](Scripts/Utility/Core/GameRule.cs) |
| 입력 처리 | [`Player/PlayerInputHandler.cs`](Scripts/Player/PlayerInputHandler.cs) |

## 사용한 외부 에셋 (저장소 미포함)

빌드에는 아래 에셋이 사용되었으며, 재배포가 불가능하여 저장소에는 포함하지 않았습니다.

- Toon Shaders Pro (URP), AOS Fog of War (FischlWorks), Hand Painted Stone Texture — Unity Asset Store EULA
- SD Unity-Chan Haon Custom — Unity-Chan License (UCL 2.0)

캐릭터: © Unity Technologies Japan/UCL
