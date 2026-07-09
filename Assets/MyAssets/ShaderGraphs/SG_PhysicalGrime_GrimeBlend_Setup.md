# SG_PhysicalGrime — `_GrimeBlend` 셋업 가이드 (단일 머테리얼 방식)

> 처음에는 Physical_Normal 위에 Grime 머테리얼을 두 번째 슬롯으로 겹쳐서 Transparent로 블렌딩하는 방식을 시도했지만,
> Opaque 두 장을 같은 메쉬에 겹치면 나중에 그려지는 쪽이 깊이 테스트를 통과해 그냥 덮어써버려서 블렌딩 없이 위 슬롯만
> 보이는 문제가 있었습니다. 그래서 **셰이더 그래프 안에서 직접 원본 돌벽 텍스처를 샘플링해 그림 패턴과 Lerp로 섞는 단일
> 머테리얼 방식**으로 바꿨습니다. 슬롯도 하나, Opaque 그대로, Transparent/Alpha 설정도 필요 없습니다.

## 이미 되어 있는 것 (코드/에셋 쪽 작업 완료)

- `Wall_Physical.prefab`의 MeshRenderer 머테리얼 슬롯은 이제 **1개**: `Physical_Grime_Mat.mat` (`SG_PhysicalGrime` 셰이더 사용, 완성되면 이게 곧 벽의 유일한 머테리얼이 됨)
- `Physical_Grime_Mat.mat`에 텍스처 슬롯 `_BaseTex`를 미리 채워둠 — 기존 `Physical_Normal.mat`이 쓰던 것과 같은 돌벽 베이스 텍스처(`Stone_Base` 계열)를 가리킴. 아래에서 만들 `_BaseTex` 프로퍼티 Reference 이름과 정확히 맞춰뒀으니 그래프에서 이름만 똑같이 만들면 자동으로 이 텍스처가 물려있는 채로 뜸.
- `MazeLayerManager.cs`에 30초 주기 로직이 있음 (`RestartPhysicalWallGrimeCycle` 등):
  - 새 판이 시작될 때(`SetLayersAndMazeGenerate()`) 블렌드를 0으로 리셋하고 사이클을 시작
  - `_grimeCycleInterval`(기본 30초)마다 `_grimeStep`(기본 0.25)만큼 블렌드를 늘리고, `_grimeFadeDuration`(기본 3초) 동안 부드럽게 보간
  - 블렌드가 1(완전히 덮임)에 도달하면 정지, 게임 클리어/오버 시에도 정지, 일시정지 중엔 같이 멈춤
  - 매 프레임 `Physical_Grime_Mat` 머테리얼(공유 에셋) 하나에만 `SetFloat("_GrimeBlend", value)`를 호출 — Physical 레이어의 모든 벽이 같은 머테리얼을 참조하므로 한 번에 전부 반영됨
  - 씬의 `MazeLayerManager` 컴포넌트 `Physical Wall Grime` 섹션에도 이미 이 머테리얼이 연결되어 있음

**남은 작업은 Shader Graph 에디터에서 원본 텍스처를 샘플링해 그림 패턴과 섞고, `_GrimeBlend`로 그 비율을 조절하도록 그래프를 연결하는 것뿐입니다.**

## 현재 그래프 상태 참고

`SG_PhysicalGrime.shadergraph`는 프로퍼티가 하나도 노출되어 있지 않은 완전 절차적 그래프입니다. 월드 좌표 기반 `Simple Noise` → `Voronoi`로 얼룩/균열 패턴을 만들고, 지금 `Base Color` 블록에는 그 패턴(보라/적갈색 Lerp)이 그대로 꽂혀 있습니다. 이제 이 패턴을 "덮는 오염 무늬"로 재해석해서, 원본 텍스처와 `_GrimeBlend` 비율로 섞을 겁니다.

## 작업 순서

1. **`SG_PhysicalGrime.shadergraph` 더블클릭**해서 Shader Graph 에디터 열기.

2. **Blackboard에 프로퍼티 2개 추가**
   - `+` → `Texture2D` 선택 → 이름은 자유(예: `Base Texture`), **Reference를 정확히 `_BaseTex`로 수정** (머테리얼에 이미 이 이름으로 텍스처를 채워뒀기 때문에 정확히 일치해야 자동으로 물려있는 텍스처가 인식됨)
   - `+` → `Float` 선택 → 이름 자유(예: `Grime Blend`), **Reference를 정확히 `_GrimeBlend`로 수정** (C# 코드가 `Material.SetFloat("_GrimeBlend", ...)`로 이 이름을 그대로 참조함). Default `0`, Mode `Slider`, Min `0` / Max `1`

3. **원본 텍스처 샘플링**
   - `Sample Texture 2D` 노드 생성 → `Texture` 입력에 Blackboard의 `_BaseTex` 프로퍼티 연결
   - (UV 입력은 비워두면 기본 UV0가 자동으로 들어감)
   - 결과 `RGBA` 출력의 `RGB`가 원본 돌벽 색상

4. **번져가는 오염 마스크 만들기 (기존 Simple Noise 재사용)**
   그래프에 이미 있는 `Simple Noise` 노드(왼쪽, `Voronoi`의 UV 입력과 보라/적갈색 `Lerp`의 T 입력으로 이어지는 노드)의 `Out` 출력을 재사용합니다. 이 값은 벽 표면 전체에 0~1로 흩어져 있어서, `_GrimeBlend`와 비교하는 문턱값으로 쓰면 "먼저 오염되는 부분 → 점점 넓게 번지는" 자연스러운 그림을 만들 수 있습니다.

   - `Add` 노드: A = `_GrimeBlend` 프로퍼티, B = 상수 `0.05` → `Edge2`
   - `Subtract` 노드: A = `_GrimeBlend` 프로퍼티, B = 상수 `0.05` → `Edge1`
   - **새 `Smoothstep` 노드** 추가 (기존 Emission 쪽에 연결된 Smoothstep은 건드리지 말 것):
     - `Edge1` ← 위 `Subtract` 결과
     - `Edge2` ← 위 `Add` 결과
     - `In` ← `Simple Noise`의 `Out`
   - 이 노드의 `Out`이 "이 픽셀이 얼마나 오염됐는지"(0=깨끗, 1=완전히 오염) 마스크

5. **원본 텍스처와 그림 패턴을 마스크로 Lerp**
   - `Lerp` 노드 생성:
     - `A` ← `Sample Texture 2D`의 `RGB` (원본 돌벽)
     - `B` ← 기존에 `Base Color` 블록에 꽂혀 있던 그 얼룩 패턴 출력(보라/적갈색 `Lerp` 결과, 즉 지금 Base Color에 연결된 선의 시작점)
     - `T` ← 4번에서 만든 새 `Smoothstep`의 `Out`
   - 이 `Lerp`의 결과를 **`Base Color` 블록**에 연결 (기존 연결을 이걸로 교체)

6. **Emission도 같이 페이드(선택, 권장)**
   - 기존 `Emission` 값(균열 빛나는 효과)에 `Multiply` 노드를 하나 추가해서 4번 마스크(`Smoothstep.Out`)를 곱해주면, 오염이 안 된 부분에서는 빛나는 균열도 같이 안 보이게 됨. 안 해도 크게 어색하진 않지만 더 자연스러움.

7. **저장** (Ctrl+S).

8. **확인**
   - `Assets/MyAssets/Materials/Physical_Grime_Mat.mat` 선택 → 인스펙터에 `Base Texture`(텍스처가 이미 채워져 있어야 함)와 `Grime Blend` 슬라이더가 보이면 연결 성공
   - `Grime Blend`를 0→1로 드래그하면서 씬 뷰에서 벽이 원본 돌벽 → 점점 오염된 패턴으로 바뀌는지 확인. 확인 후 슬라이더는 다시 `0`으로
   - Play 모드로 들어가서 Physical 레이어 벽이 처음엔 원본 돌벽 그대로였다가, 기본 설정 기준 30초마다 한 단계씩(총 4단계, 약 120초) 점점 오염되어 가는지 확인

## 튜닝 포인트

- `MazeLayerManager`의 `_grimeCycleInterval`(주기) / `_grimeStep`(한 단계 증가량) / `_grimeFadeDuration`(한 단계가 부드럽게 올라가는 시간)은 Inspector에서 바로 조정 가능
- 오염 패턴 크기는 `Simple Noise` 노드의 `Scale`(현재 8)로 조정
- 5번 문턱 폭(`0.05`)을 늘리면 경계가 부드럽게, 줄이면 또렷하게 번짐
