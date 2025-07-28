<!--- Autodesk.DesignScript.Geometry.TSpline.TSplineVertex.FunctionalValence --->
<!--- N44VZ3AJYWSL6V3DZOJYGO3ER47KV2Q6UNXWX7N6U47NDLFO3TBQ --->
## In-Depth
정점의 기능적 Valence는 인접한 모서리의 단순한 개수를 넘어 주변 영역의 정점 혼합에 영향을 미치는 가상 그리드 선을 고려합니다. 이를 통해 변형 및 미세 조정 작업 중에 정점 및 해당 모서리가 표면에 미치는 영향을 더욱 미묘한 수준까지 파악할 수 있습니다.
일반 정점과 T 점에서 사용하는 경우 `TSplineVertex.FunctionalValence` 노드는 값 "4"를 반환하며, 이는 표면이 그리드 모양의 스플라인으로 유도된다는 의미입니다. 값이 "4" 이외인 기능적 Valence는 정점이 별 점이고 정점 주위의 혼합이 덜 매끄러워진다는 의미입니다.

아래 예에서는 `TSplineVertex.FunctionalValence`가 T-Spline 평면 표면의 두 개의 T 점 정점에서 사용됩니다. `TSplineVertex.Valence` 노드는 값 3을 반환하며, T 점의 경우 선택한 정점의 기능적 Valence는 4를 반환합니다. `TSpline Vertex.UVNFrame` 및 `TSplineUVNFrame.Position` 노드는 분석할 정점의 위치를 시각화하는 데 사용됩니다.

## 예제 파일

![Example](./N44VZ3AJYWSL6V3DZOJYGO3ER47KV2Q6UNXWX7N6U47NDLFO3TBQ_img.jpg)
