# 게임 엔진 포트폴리오 과제

**유니티 버전:** 6.4.9.f1

---

## 프로젝트 개요

플레이어는 미로에 갇혀 있다. 미로 곳곳에 흩어진 **조각(열쇠)** 을 모두 모으면, **Goal Point** 가 나타난다. 그  **Goal Point**에 도달하면 미로에서 탈출할 수 있다.

미로는 특정 커맨드를 입력하여 전환할 수 있다.
- **Physical**
- **Arcane**

두 레이어는 완전히 다른 벽 구조를 갖고 있어서, 한쪽에서 막힌 길이 다른 쪽에서는 열려 있을 수 있다. 플레이어는 언제든 두 레이어를 오갈 수 있고, 미로를 떠도는 몬스터를 피해 길을 찾아 조각을 모아야 한다.

| 항목 | 내용 |
|---|---|
| 엔진 | Unity 6 |
| 렌더 파이프라인 | URP |
| 장르 | 탑다운 미로 탐험 / 추격 회피 |
| 미로 크기 | 20 x 20 (고정) |

---

## 시연 방법

1. Unity 6(URP)로 프로젝트 열기(Version:6.4.9f1)
2. `Assets/MyAssets/Scenes/` 의 `GameEngineAssignment` 씬 열고 Play
3. 타이틀 화면에서 **Start** 버튼 클릭 → 20x20 고정 크기 미로로 바로 게임 시작

---

## 게임 목표 / 클리어 조건

1. 미로 곳곳에 흩어진 **열쇠(빛나는 구체) 5개**를 전부 수집
2. 5개를 모두 모으면 미로의 오른쪽 위 구석에 **Goal Point(황금색 구체)** 생성
3. **Goal Point(황금색 구체)** 에 도달하면 **클리어**
4. 몬스터에게 발각되면 플레이어를 추적 → 공격범위 안에 들어와 공격당해 **HP(3)가 0이 되면 게임 오버**
5. 클리어/게임 오버 후 결과 화면에서 **Replay**로 재도전 가능

---

## 조작법

| 입력 | 기능 |
|---|---|
| `W` `D` `A` `S` / 방향키 | 상하좌우 이동 |
| `Tab` | 미로 레이어 전환 (Physical ↔ Arcane) |

---

## 실행 화면

### 게임 실행
<img src="Assets/MyAssets/Screenshots/Start.png" width="800">

### 게임 플레이 
<img src="Assets/MyAssets/Screenshots/Runtime.png" width="800">

### 게임 클리어
<img src="Assets/MyAssets/Screenshots/End.png" width="800">

---

## 사용한 엔진 기능

| 분류 | 적용 내용 |
|---|---|
| **Cinemachine** | `CinemachineBrain` + `CinemachineCamera`, 시작 인트로 카메라와 쿼터뷰 추적 카메라를 우선순위 기반 블렌딩으로 카메라 전환 연출, `Cinemachine Impulse`로 몬스터에게 피격 시 카메라 흔들림 |
| **조명 / 머티리얼** | Physical/Arcane 벽 머티리얼(ShaderGraph → `SG_PhysicalGrime`, `SG_ArcaneNoise`), Key/Goal Point 용 발광 머티리얼(ShaderGraph → `SG_GlowOrb`), 실시간 Point Light pulse(`PulsingLight.cs`), 레이어 전환에 맞춘 Directional Light·대기 Fog 색 전환(`LayerLightingController.cs`), 화면 일렁임 전환 효과(ShaderGraph → `SG_LayerTransitionRipple`) |
| **이동/경로 시스템** | Player: `CharacterController` 기반 이동 / Monster: `NavMeshAgent` + `NavMeshSurface`(레이어별 분리 베이크) 로 미로 Patrol, 플레이어 추적 |
| **UI** | Canvas 하위 Title/InGame/Result UI Panel을 `GameUIController.cs`가 조율(패널 전환·이벤트 연결), `DamageflashUI.cs`로 피격 시 화면 빨간색 페이드 연출 |
| **사운드** | `SoundManager` + `SoundLibrary`(ScriptableObject, AudioClip들을 보관) |
| **물리 Trigger** | 열쇠 회수, 골 포인트 도달, 몬스터 공격 범위 판정 |
| **VFX** | Particle System(사방으로 흩어지는 입자) |
| **리소스 관리** | MyAssets → Materials / Prefabs / Scripts / ShaderGraphs 등의 폴더 분리 후 관리, External 에셋(AOS Fog War)은 따로 건드리지 않음(임포트 경로 그대로 보존)|

### 추가로 사용한 시스템
- **Input System** — `PlayerInput`(Invoke C Sharp Events) 기반 이벤트 입력 처리
- **Awaitable 비동기** — 레이어 전환 시퀀스, 카메라 인트로 전환 등 시간 기반 연출 처리
- **FSM** — 몬스터 AI(Idle / Chase / Attack) 상태 관리
- **External Asset: AOS Fog War** — 플레이어 시야 기반 안개 시스템

---

## 리소스 구성

```
Assets/
	AOSFogWar/           # 외부 에셋
	MyAssets/			 # 직접 작업한 작업물
  		Materials/            	 # Materials
  		Prefabs/				 # Prefabs
		Scenes/	   				 # GameAssignment Scene
		Screenshots/ 			 # 인 게임 Screen Shots
		ScriptableObjectAssets/  # SoundLibrary Asset
  		Scripts/
			Monster/             # 몬스터 관련 .cs
			Obsolete/            # AStarPathfinder, FollowCamera ([Obsolete] 처리한 .cs)
			Player/              # 플레이어 관련 .cs
			ScriptableObject/	 # 스크립터블 오브젝트 .cs(SoundLibrary)
			UI/					 # UI 관련 .cs
    		Utility/             # 이 외의 모든 .cs
				Manager/		 # Utility안에서도 매니저는 따로 폴더로 구분    		
  		ShaderGraphs/            # Shader Graphs
		SoundClips/				 # AudioClips
```
---

게임 컨셉 : 악몽에 갇힌 플레이어, 미로에서 열쇠(영혼? 조각? 명칭은 안정함)를 찾아 악몽에서 빠져나가기(Goal Point)

추가되는 룰:
	노멀 난이도	
		현재 상태 유지(벽 오브젝트만 좀 다르게 바꾸고 말듯, 현재의 기형적인 무늬가 나오는 Physical Wall을 하드 난이도에서 활용)
	하드 난이도
		타임 제한 설정(일단은 3분으로 설정, 추후 조정 가능)
		미로를 전환할때 리스크 증가
			Arcane 미로로 전환 시 몬스터의 이동속도가 플레이어보다 빨라짐
			어디에 있든 항상 플레이어를 감지함
			플레이어 정신력? 게이지 만들어서 Arcane미로에 오래 머물수록 정신력이 깎이고 정신력 게이지가 0이되면 곧바로 게임이 종료되게? 정신력 게이지는 Physical 상태일때는 조금씩 회복
		시간이 지남에 따라 Physical Maze에도 영향이 감
			30초? 혹은 1분?이 지날때마다 조금씩 벽에 기형적인 무늬(지금 Physical에 적용된 머테리얼)가 조금씩 늘어나고 마지막30초에는 현재 Physical에 적용된 머테리얼이 그대로 적용
			위에 처럼 바뀔때마다 화면도 조금씩 흔들리게 해서 플레이어 이동 방해, 마지막 30초쯤에는 플레이하는 유저가 멀미가 일정도로 흔들리게 or 화면 일러이는 효과(우리가 만든 레이어 전환 쉐이더 그래프 활용)
			브금도 점점 고조되게 바꿀듯

몬스터, 플레이어 캐릭터 에셋과 적용할 애니메이션 추가
	몬스터 에셋도 카툰렌더링이나 현재 분위기에 맞는 애니메이션 찾음
	표정이 바뀌는 SD캐릭터 찾음 
		- Haon SD series Bundle
		- SD chan Animation bundle
			둘이 연관되어서 모델링, 애니메이션 매칭 쉬움
		
	

게임 인트로 연출 제작할듯
	캐릭터 모델링, 표정이 다 들어있어서 이걸로 침대에 누워있는 캐릭터가 고개를 찌푸리며 악몽을 꾸는 연출을 타임라인으로 인트로 영상? 연출? 만들어서 게임 플레이 도입부에 넣고
	Title UI로 넘어가기
		title UI : 게임 이름 / 시작버튼 / 종료 버튼 / 옵션 설정?버튼
		Select UI : 난이도 설정(노말, 하드), 캐릭터 설정(시간이 되면 추가)

	게임 시작 시 -> 화면으로 빨려들어가는 연출? 후 현재 제작한 인트로 화면에서 플레이어 추적 카메라로 전환되는 연출 
	-> 카메라 전환이 완료되면 미로에 쓰러져있던 캐릭터가 일어나서 주변을 두리번 거리는 애니메이션 재생 후 게임 시간이 흘러가면서 게임 시작
	
벽 오브젝트 꾸미기
	좀더 카툰렌더링 모델에 어울리게 장난감이나 색다른 블럭을 Wall로 설정후, 우리가 만든 일그러지는 패턴 쉐이더로 시간이 지남에 따라 벽에 기하학적인 무늬가 생기게 해서 공포 분위기 연출
	

UI 꾸미기
	카툰렌더링 모델을 사용하므로 공포 분위기와 카툰느낌이 어우러지게 바꿀듯


Idle
	Fsad - 100
	Fhide - 0 ~ 100 (눈 깜빡거리기)

	몬스터에게 걸렸을 때
	Fsad - 0
	Fhide - 0
	Fdam - 100


Move(Blend Tree:Walk + Run)
	Fsad - 100
	Fhide - 0 ~ 100 (눈 깜빡거리기)

	몬스터에게 걸렸을 때
	Fsad - 0
	Fhide - 0
	Fdam - 100



누워서 고통스러워 하는 연출
	페이스 하이드100
		새드 100

애니메이션 -> StandB

	--> 이렇게하면 찡그린 표정으로 고개 좌우로 움직임 -> 침대 위에 눕혀놓고 이렇게 설정하고 카메라를 얼굴쪽으로 조금씩 확대하면 연출될듯

	



