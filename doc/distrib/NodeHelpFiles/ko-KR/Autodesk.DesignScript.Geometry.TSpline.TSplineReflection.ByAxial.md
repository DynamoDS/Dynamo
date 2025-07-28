## In-Depth
`TSplineReflection.ByAxial`는 `TSplineSurface.AddReflections` 노드의 입력으로 사용할 수 있는 `TSplineReflection` 객체를 반환합니다.
`TSplineReflection.ByAxial` 노드의 입력은 대칭 기준면으로 사용되는 평면입니다. TSplineInitialSymmetry와 매우 유사하게 TSplineReflection는 TSplineSurface에 대해 설정되면 모든 이후 작업 및 변경 사항에 영향을 줍니다.

아래 예에서는 `TSplineReflection.ByAxial`이 T-Spline 원추의 맨 위에 배치된 TSplineReflection을 작성하는 데 사용됩니다. 그런 다음 원추를 반사하고 새 T-Spline 표면을 반환하기 위해 반사가 `TSplineSurface.AddReflections 노드의 입력으로 사용됩니다.

## 예제 파일

![Example](./Autodesk.DesignScript.Geometry.TSpline.TSplineReflection.ByAxial_img.jpg)
