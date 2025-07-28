## 상세

아래 예에서는 T-Spline 표면이 `TSplineSurface.CreateMatch(tSplineSurface,tsEdges,brepEdges)` 노드를 사용하여 BRep 표면의 모서리와 일치됩니다. 노드에 필요한 최소 입력은 베이스 `tSplineSurface`, `tsEdges` 입력에서 제공되는 표면 모서리 세트, 그리고 `brepEdges` 입력에서 제공되는 모서리 또는 모서리 리스트입니다. 다음 입력은 일치 항목의 매개변수를 제어합니다.
- `continuity`을 사용하여 일치 항목에 대한 연속성 유형을 설정할 수 있습니다. 입력에는 G0 위치, G1 접선 및 G2 곡률 연속성에 해당하는 0, 1 또는 2가 필요합니다.
- `useArcLength`는 선형 유형 옵션을 제어합니다. True로 설정하면 사용된 선형 유형은 호 길이입니다. 이 선형은 T-Spline 표면의 각 점과 곡선의 해당 점 간의 물리적 거리를 최소화합니다. False 입력이 제공되면 선형 유형은 파라메트릭입니다. T-Spline 표면의 각 점은 일치 대상 곡선을 따라 비교 가능한 파라메트릭 거리의 점에 일치됩니다.
- True로 설정된 경우 `useRefinement`는 지정된 `refinementTolerance` 내 대상과 일치시키기 위해 표면에 제어점을 추가합니다
- `numRefinementSteps` is the maximum number of times that the base T-Spline surface is subdivided
while attempting to reach `refinementTolerance`. Both `numRefinementSteps` and `refinementTolerance` will be ignored if the `useRefinement` is set to False.
- `usePropagation` controls how much of the surface is affected by the match. When set to False, the surface is minimally affected. When set to True, the surface is affected within the provided `widthOfPropagation` distance.
- `scale` is the Tangency Scale which affects results for G1 and G2 continuity.
- `flipSourceTargetAlignment` reverses the alignment direction.


## 예제 파일

![Example](./BUMI5UR5LLKRXP5CUH46L62SN6YIVFGB6FU2PUTKTVDJMTXWXI5Q_img.gif)
