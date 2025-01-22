<!--- Autodesk.DesignScript.Geometry.TSpline.TSplineSurface.ByPlaneOriginNormal --->
<!--- DWYJGQGBW2MD57NAYFKR3UEMPTHASLR2IV2X2SUK3OKI35GCEVHQ --->
## In-Depth
`TSplineSurface.ByPlaneOriginNormal`은 원점 및 법선 벡터를 사용하여 T-Spline 원형 평면 표면을 생성합니다. T-Spline 평면을 작성하려면 노드가 다음 입력을 사용합니다.
- `origin`: 평면의 원점을 정의하는 점.
- `normal`: 작성된 평면의 법선 방향을 지정하는 벡터.
- `minCorner` 및 `maxCorner`: X 및 Y 값(Z 좌표는 무시됨)을 가진 점으로 표현된 평면의 모서리. 이러한 모서리는 XY 평면으로 변환되는 경우 출력 T-Spline 표면의 범위를 나타냅니다. `minCorner` 및 `maxCorner` 점은 3D의 모서리 정점과 일치하지 않아도 됩니다. 예를 들어 `minCorner`는 (0,0)으로 설정되고 `maxCorner`는 (5,10)인 경우, 평면 폭과 길이는 각각 5와 10이 됩니다.
- `xSpan` 및 `ySpan`: 평면의 폭 및 길이 스팬/분할 수
- `symmetry`: 형상이 X, Y 및 Z축을 기준으로 대칭인지 여부
- `InSmoothMode`: 결과 형상이 매끄럽게 모드 또는 상자 모드로 표시되는지 여부

아래 예에서는 T-Spline 평면형 표면이 제공된 원점과 X축의 벡터인 법선을 사용하여 작성됩니다. 표면의 크기는 `minCorner` 및 `maxCorner` 입력으로 사용되는 두 점으로 제어됩니다.

## 예제 파일

![Example](./DWYJGQGBW2MD57NAYFKR3UEMPTHASLR2IV2X2SUK3OKI35GCEVHQ_img.jpg)
