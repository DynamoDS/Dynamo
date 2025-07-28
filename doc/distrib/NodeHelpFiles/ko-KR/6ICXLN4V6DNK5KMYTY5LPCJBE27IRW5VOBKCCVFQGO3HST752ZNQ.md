## 상세

아래 예에서는 T-Spline 표면이
`TSplineSurface.CreateMatch(tSplineSurface,tsEdges,curves)` 노드를 사용하여 NURBS 곡선과 일치됩니다. 노드에 필요한 최소 입력은
베이스 `tSplineSurface`, `tsEdges` 입력에서 제공되는 표면의 모서리 세트, 그리고 곡선 또는
곡선 리스트입니다.
다음 입력은 일치 항목의 매개변수를 제어합니다.
- `continuity`를 통해 일치 항목에 대한 연속성 유형을 설정할 수 있습니다. 입력에는 G0 위치, G1 접선 및 G2 곡률 연속성에 해당하는 0, 1 또는 2 값이 필요합니다. 그러나 곡선과 표면을 일치시키려면 G0(입력값 0)만 사용할 수 있습니다.
- `useArcLength`는 선형 유형 옵션을 제어합니다. True로 설정하면 사용되는 선형 유형은 호
길이입니다. 이 선형은 T-Spline 표면의 각 점과
곡선의 해당 점 사이의 물리적 거리를 최소화합니다. False 입력이 제공되면 선형 유형은 파라메트릭입니다.
T-Spline 표면의 각 점은 일치 대상 곡선을 따라 비교 가능한 파라메트릭 거리의 점에
일치됩니다.
- True로 설정된 경우 `useRefinement`는 지정된 `refinementTolerance` 내 대상과 일치시키기 위해
표면에 제어점을 추가합니다
- `numRefinementSteps`는 `refinementTolerance`에 도달하려고 하는 동안 베이스 T-Spline 표면이
세분화된 최대 횟수입니다.`useRefinement`가 False로 설정되면 `numRefinementSteps` 및 `refinementTolerance` 모두 무시됩니다.
- `usePropagation`은 표면이 일치 항목에 얼마나 영향을 받는지를 제어합니다. False로 설정되면 표면이 최소한으로 영향을 받습니다. True로 설정되면 표면이 제공된 `widthOfPropagation` 거리 내에서 영향을 받습니다.
- `scale`은 G1 및 G2 연속성의 결과에 영향을 주는 접선 축척입니다.
- `flipSourceTargetAlignment`는 선형 방향을 반전합니다.


## 예제 파일

![Example](./6ICXLN4V6DNK5KMYTY5LPCJBE27IRW5VOBKCCVFQGO3HST752ZNQ_img.gif)
