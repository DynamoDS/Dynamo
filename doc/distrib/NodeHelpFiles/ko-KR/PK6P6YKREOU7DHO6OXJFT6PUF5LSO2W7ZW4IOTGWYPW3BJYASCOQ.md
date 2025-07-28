<!--- Autodesk.DesignScript.Geometry.TSpline.TSplineInitialSymmetry.ByRadial --->
<!--- PK6P6YKREOU7DHO6OXJFT6PUF5LSO2W7ZW4IOTGWYPW3BJYASCOQ --->
## In-Depth
`TSplineInitialSymmetry.ByRadial`은 T-Spline 형상에 방사형 대칭이 있는지를 정의합니다. 방사형 대칭은 이를 허용하는 T-Spline 원형(원추, 구, 회전, 원환)에 대해서만 적용될 수 있습니다. 방사형 대칭은 T-Spline 형상 작성 시 설정되면 모든 후속 작업 및 변경 사항에 영향을 줍니다.

대칭을 적용하려면 원하는 `SymmetricFaces` 수를 정의해야 하고 1은 최소값입니다. T-Spline 표면은 시작되는 반지름 및 높이 스팬 수와 관계없이 선택한 `SymmetricFaces` 수로 더 분할됩니다.

아래 예에서는 `TSplineSurface.ByConePointsRadii`가 작성되고 방사형 대칭이 `TSplineInitialSymmetry.ByRadial` 노드를 사용하여 적용됩니다. 그런 다음 `TSplineTopology.RegularFaces` 및 `TSplineSurface.ExtrudeFaces` 노드가 각각 T-Spline 표면의 면을 선택하고 돌출시키는 데 사용됩니다. 돌출은 대칭으로 적용되고 대칭 면 수에 대한 슬라이더는 방사형 스팬이 세분화되는 방법을 보여줍니다.

## 예제 파일

![Example](./PK6P6YKREOU7DHO6OXJFT6PUF5LSO2W7ZW4IOTGWYPW3BJYASCOQ_img.gif)
