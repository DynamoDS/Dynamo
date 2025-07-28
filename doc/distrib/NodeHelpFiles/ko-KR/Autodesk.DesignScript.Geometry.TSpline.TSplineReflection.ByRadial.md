## In-Depth
`TSplineReflection.ByRadial`은 `TSplineReflection.AddReflections` 노드의 입력으로 사용할 수 있는 `TSplineReflection` 객체를 반환합니다. 노드는 평면을 입력으로 간주하며, 평면의 법선은 형상 회전을 위한 축 역할을 합니다. TSplineInitialSymmetry와 매우 유사하게 TSplineReflection은 TSplineSurface 작성 시 설정되면 모든 이후 작업 및 변경 사항에 영향을 줍니다.

아래 예에서는 `TSplineReflection.ByRadial`이 T-Spline 표면의 반사를 정의하는 데 사용됩니다. `segmentsCount` 및 `segmentAngle` 입력은 지정된 평면의 법선 주위에서 형상이 반사되는 방식을 제어하는 데 사용됩니다. 그런 다음 새 T-Spline 표면을 작성하기 위해 노드의 출력이 `TSplineSurface.AddReflections` 노드의 입력으로 사용됩니다.

## 예제 파일

![Example](./Autodesk.DesignScript.Geometry.TSpline.TSplineReflection.ByRadial_img.gif)
