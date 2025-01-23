## 상세
닫힌 표면은 개구부 또는 경계 없이 완전한 모양을 형성하는 표면입니다.
아래 예에서는 T-Spline 구가 `TSplineSurface.BySphereCenterPointRadius`를 통해 생성되고 열려 있는지를 확인하는 `TSplineSurface.IsClosed`를 사용하여 검사되며, 여기서는 음수 결과를 반환합니다. 이는 T-Spline 구가 닫혀 있는 것처럼 보이더라도 실제로는 여러 모서리 및 정점이 한 점에 적층되는 극에서 열려 있기 때문입니다.

그런 다음 T-Spline 구의 간격이 `TSplineSurface.FillHole` 노드를 사용하여 채워지고, 이로 인해 표면이 채워지는 위치에서 약간의 변형이 발생합니다. `TSplineSurface.IsClosed` 노드를 통해 다시 확인하면 이제 닫혔음을 의미하는 양수 결과를 생성합니다.
___
## 예제 파일

![TSplineSurface.IsClosed](./Autodesk.DesignScript.Geometry.TSpline.TSplineSurface.IsClosed_img.jpg)
