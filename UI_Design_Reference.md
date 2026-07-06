# UI 디자인 레퍼런스 — ECHOES OF ME (타이틀 화면 기준)

## 확정된 디자인 방향

레퍼런스 게임("ECHOES OF ME")의 미니멀한 구조를 뼈대로,
공포+카툰 컨셉의 색감과 기하학적 디테일을 얹은 방향으로 확정.

---

## 컬러 팔레트

```css
--nm-bg:          #0A050F;   /* 거의 검정, 보라 기운 */
--nm-panel:       #1A0D28;   /* 패널 배경 */
--nm-border:      #3D2860;   /* 기본 테두리 */
--nm-purple:      #7B3FA0;   /* 주 포인트 색 (버튼, 서브타이틀) */
--nm-purple-dim:  #4A2260;   /* 코너 장식, 구분선 등 흐린 보라 */
--nm-dim:         #7A6890;   /* 비활성 텍스트, ghost 버튼 */
--nm-horror:      #C8B8DC;   /* 메인 텍스트 (흰색보다 약간 보라 기운) */
--nm-text:        #DDD0F0;   /* 일반 텍스트 */
--nm-red:         #C42030;   /* HARD 모드 전용 위험 색 */
--nm-red-dim:     #6B1018;   /* HARD 카드 테두리 등 */
--nm-soul:        #6BA8E8;   /* 꿈의 조각(열쇠) 아이콘 색 */
--nm-gold:        #D4A020;   /* 클리어 결과 강조 색 */
```

---

## 타이포그래피

| 용도 | 폰트 | 크기 | 굵기 | 특징 |
|---|---|---|---|---|
| 게임 제목 | Cinzel (Google Fonts) | 36px | 700 | letter-spacing: 0.12em |
| 서브타이틀 | Cinzel | 12px | 400 | letter-spacing: 0.3em, 색: --nm-purple |
| 버튼 | Cinzel | 13px | 400 | letter-spacing: 0.2em |
| HUD 수치/타이머 | Share Tech Mono | 13~22px | 400 | 모노스페이스, 기계적 느낌 |
| 레이블/배지 | Share Tech Mono | 10~11px | 400 | letter-spacing: 0.12~0.15em |

Google Fonts 로드:
```html
<link href="https://fonts.googleapis.com/css2?family=Cinzel:wght@400;700&family=Share+Tech+Mono&display=swap" rel="stylesheet">
```

---

## 시그니처 요소

### 1. 비대칭 절단 버튼 (clip-path)
모든 버튼의 핵심 — 양쪽 모서리가 비스듬히 잘린 평행사변형 형태.
둥근 모서리 대신 이 절단면이 "공간이 어긋난 악몽" 느낌을 만들어냄.

```css
clip-path: polygon(8px 0%, 100% 0%, calc(100% - 8px) 100%, 0% 100%);
```

### 2. 코너 장식 (L자 테두리)
패널/화면 네 귀퉁이에 L자 형태의 가이드라인.
실선이 아니라 "측정 도구" 같은 느낌 — 악몽 속에서 공간을 재고 있는 분위기.

```css
/* 각 코너 오브젝트 */
.corner-tl { top: 12px; left: 12px; border-width: 1px 0 0 1px; }
.corner-tr { top: 12px; right: 12px; border-width: 1px 1px 0 0; }
.corner-bl { bottom: 12px; left: 12px; border-width: 0 0 1px 1px; }
.corner-br { bottom: 12px; right: 12px; border-width: 0 1px 1px 0; }
/* 공통 */
width: 20px; height: 20px;
border-color: #4A2260;
border-style: solid;
```

### 3. 카드 절단 모서리 (난이도 선택 등)
카드는 우측 상단 모서리만 잘린 형태.

```css
clip-path: polygon(0 0, calc(100% - 12px) 0, 100% 12px, 100% 100%, 12px 100%, 0 calc(100% - 12px));
```

---

## 버튼 스타일 2종

### Primary (메인 CTA)
```css
background: #7B3FA0;
color: #C8B8DC;
border: 1px solid #7B3FA0;
clip-path: polygon(8px 0%, 100% 0%, calc(100% - 8px) 100%, 0% 100%);
```

### Ghost (보조)
```css
background: transparent;
color: #7A6890;
border: 1px solid #3D2860;
clip-path: polygon(8px 0%, 100% 0%, calc(100% - 8px) 100%, 0% 100%);
```

---

## 타이틀 화면 구조 (확정)

```
배경: #0A050F
  ├── 코너 장식 4개 (보라 L자)
  ├── 제목: "ECHOES OF ME"
  │     Cinzel 700 / 36px / #C8B8DC / letter-spacing: 0.12em
  ├── 서브타이틀: "Run and survive, away from them.."
  │     Cinzel 400 / 12px / #7B3FA0 / letter-spacing: 0.3em
  │     (여백: 서브타이틀 아래 48px)
  ├── [START] Primary 버튼
  ├── [OPTIONS] Ghost 버튼
  └── [EXIT] Ghost 버튼
```

---

## 각 화면별 포인트 색

| 화면 | 포인트 색 | 이유 |
|---|---|---|
| 타이틀 | --nm-purple | 기본 상태 |
| HUD (Physical) | --nm-purple | Physical 레이어 = 보라 기조 |
| HUD (Arcane) | --nm-purple 강조 | Arcane = 보라 더 강하게 |
| 난이도 HARD | --nm-red | 위험 신호 |
| 클리어 결과 | --nm-gold | 성취/탈출 |
| 게임오버 결과 | --nm-red | 실패/잠식 |
| 꿈의 조각 아이콘 | --nm-soul | 열쇠 비주얼과 통일 |

---

## USS 변수 선언 (UI Toolkit 적용 시)

```css
/* Variables.uss */
:root {
    --nm-bg: #0A050F;
    --nm-panel: #1A0D28;
    --nm-border: #3D2860;
    --nm-purple: #7B3FA0;
    --nm-purple-dim: #4A2260;
    --nm-dim: #7A6890;
    --nm-horror: #C8B8DC;
    --nm-text: #DDD0F0;
    --nm-red: #C42030;
    --nm-red-dim: #6B1018;
    --nm-soul: #6BA8E8;
    --nm-gold: #D4A020;
}
```

---

## 적용 금지 사항
- 둥근 모서리(border-radius) — clip-path로 대체
- 흰색(#ffffff) 텍스트 — #C8B8DC 또는 #DDD0F0 사용
- 그림자/글로우 이펙트 — 플랫 디자인 유지
- 밝은 배경 — 항상 어두운 계열 유지
