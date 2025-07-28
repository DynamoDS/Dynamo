<!--- Autodesk.DesignScript.Geometry.TSpline.TSplineSurface.ByPlaneBestFitThroughPoints --->
<!--- QMBSK3FVRYFQCMSXFIPKLNLYVY5W4C4PNN7IGZUPDZOVWUPWZNZQ --->
## In-Depth
`TSplineSurface.ByPlaneBestFitThroughPoints`는 점 리스트에서 T-Spline 원형 평면 표면을 생성합니다. T-Spline 평면을 작성하려면 노드가 다음 입력을 사용합니다.
- `points`: 평면 방향과 원점을 정의하는 점 세트. 입력 점이 단일 평면에 없는 경우 평면의 방향은 최적 맞춤을 기준으로 결정됩니다. 표면을 작성하려면 최소 3개의 점이 필요합니다.
- `minCorner` 및 `maxCorner`: X 및 Y 값(Z 좌표는 무시됨)의 점으로 표현된 평면의 모서리. 이러한 모서리는 XY 평면으로 이동하는 경우 출력 T-Spline 표면의 범위를 나타냅니다. `minCorner` 및 `maxCorner` 점은 3D의 모서리 정점과 일치하지 않아도 됩니다.
- `xSpans` and `ySpans`: number of width and length spans/divisions of the plane
- `symmetry`: whether the geometry is symmetrical with respect to its X, Y and Z axes
- `inSmoothMode`: whether the resulting geometry will appear with smooth or box mode

아래 예에서는 T-Spline 평면형 표면이 무작위로 생성된 점 리스트를 사용하여 작성됩니다. 표면의 크기는 `minCorner` 및 `maxCorner` 입력으로 사용되는 두 점으로 제어됩니다.

## 예제 파일

![Example](./QMBSK3FVRYFQCMSXFIPKLNLYVY5W4C4PNN7IGZUPDZOVWUPWZNZQ_img.jpg)
