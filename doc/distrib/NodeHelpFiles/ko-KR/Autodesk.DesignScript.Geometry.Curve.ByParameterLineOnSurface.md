## 상세
Curve by Parameter Line On Surface는 두 입력 UV 좌표 사이의 표면을 따라 선을 작성합니다. 아래 예에서는 먼저 점의 그리드를 작성한 다음 임의의 양만큼 Z 방향으로 변환합니다. 이러한 점은 NurbsSurface.ByPoints 노드를 사용하여 표면을 작성하는 데 사용됩니다. 이 표면은 ByParameterLineOnSurface 노드의 baseSurface로 사용됩니다. 숫자 슬라이더 세트는 두 UV.ByCoordinates 노드의 U 및 V 입력을 조정하는 데 사용되며, 이러한 노드는 표면에서 선의 시작점과 끝점을 결정하는 데 사용됩니다.
___
## 예제 파일

![ByParameterLineOnSurface](./Autodesk.DesignScript.Geometry.Curve.ByParameterLineOnSurface_img.jpg)

