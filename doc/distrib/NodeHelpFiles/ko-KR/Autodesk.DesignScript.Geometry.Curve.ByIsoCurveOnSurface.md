## 상세
Curve by IsoCurve on Surface는 U 또는 V 방향을 지정하고 곡선을 작성할 반대 방향으로 매개변수를 지정하여 표면에서 ISO 곡선을 작성합니다. '방향' 입력에 따라 작성할 ISO 곡선의 방향이 결정됩니다. 값 1은 U 방향에 해당하고 값 0은 V 방향에 해당합니다. 아래 예에서는 먼저 점의 그리드를 작성한 다음 Z 방향으로 임의의 양만큼 변환합니다. 이러한 점은 NurbsSurface.ByPoints 노드를 사용하여 표면을 작성하는 데 사용됩니다. 이 표면은 ByIsoCurveOnSurface 노드의 baseSurface로 사용됩니다. 0~1 범위 및 1 단계로 설정된 숫자 슬라이더는 ISO 곡선을 U 또는 V 방향으로 추출할지 여부를 제어하는 데 사용됩니다. 두 번째 숫자 슬라이더는 ISO 곡선이 추출되는 매개변수를 결정하는 데 사용됩니다.
___
## 예제 파일

![ByIsoCurveOnSurface](./Autodesk.DesignScript.Geometry.Curve.ByIsoCurveOnSurface_img.jpg)

