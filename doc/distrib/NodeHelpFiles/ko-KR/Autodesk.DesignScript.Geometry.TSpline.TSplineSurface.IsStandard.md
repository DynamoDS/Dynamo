## 상세
T-Spline 표면은 모든 T 점이 둘 이상의 Iso 곡선에 의해 별 점에서 분리되는 경우 표준입니다. T-Spline 표면을 NURBS 표면으로 변환하기 위해서는 표준화가 필요합니다.

아래 예에서는 `TSplineSurface.ByBoxLengths`를 통해 생성된 T-Spline 표면에 세분화된 면 중 하나가 있습니다. `TSplineSurface.IsStandard`는 표면이 표준인지 확인하는 데 사용되지만, 음수 결과를 생성합니다.
그런 다음 `TSplineSurface.Standardiz`가 표면을 표준화하는 데 사용됩니다. 새 제어점은 표면의 모양을 변경하지 않고 적용됩니다. 결과 표면은 현재 표준인지 확인하는 `TSplineSurface.IsStandard`를 사용하여 확인됩니다.
`TSplineFace.UVNFrame` 및 `TSplineUVNFrame.Position` 노드는 표면에서 세분화된 면을 강조 표시하는 데 사용됩니다.
___
## 예제 파일

![TSplineSurface.IsStandard](./Autodesk.DesignScript.Geometry.TSpline.TSplineSurface.IsStandard_img.jpg)
