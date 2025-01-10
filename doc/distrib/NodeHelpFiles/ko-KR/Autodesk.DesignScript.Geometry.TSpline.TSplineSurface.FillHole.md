## 상세
아래 예에서는 T-Spline 원통형 표면의 간격이 `TSplineSurface.FillHole` 노드를 사용하여 채워지고, 여기에는 다음 입력이 필요합니다.
- `edges`: 채울 T-Spline 표면에서 선택된 여러 경계 모서리
- `fillMethod`: 채우기 방법을 나타내는 0에서 3 사이의 숫자 값:
    * 0은 다듬기로 구멍을 채웁니다
    * 1은 단일 다각형 면으로 구멍을 채웁니다
    * 2는 삼각형 면이 모서리 쪽으로 퍼지는 구멍의 중심에 점을 작성합니다
    * 3은 방법 2와 유사하지만, 중심 정점이 맨 위에 적층되는 대신 하나의 정점으로 용접된다는 차이점이 있습니다.
- `keepSubdCreases`: 하위 각진 부분이 유지되는지 여부를 나타내는 부울 값.
___
## 예제 파일

![TSplineSurface.FillHole](./Autodesk.DesignScript.Geometry.TSpline.TSplineSurface.FillHole_img.gif)
