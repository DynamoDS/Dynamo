## 상세
'TSplineInitialSymmetry.ByAxial'은 T-Spline 형상에 선택한 축(x, y, z)을 따라 대칭이 있는지를 정의합니다. 대칭은 한 축, 두 축 또는 세 축 모두에서 발생할 수 있습니다. 대칭은 T-Spline 형상 작성 시 설정되면 이후의 모든 작업 및 변경 사항에 영향을 줍니다.

아래 예에서는 'TSplineSurface.ByBoxCorners'가 T-Spline 표면을 작성하는 데 사용됩니다. 이 노드의 입력 중 'TSplineInitialSymmetry.ByAxial'이 표면의 초기 대칭을 정의하는 데 사용됩니다. 그런 다음 'TSplineTopology.RegularFaces' 및 'TSplineSurface.ExtrudeFaces'가 각각 T-Spline 표면의 면을 선택하고 돌출시키는 데 사용됩니다. 이후 돌출 작업은 'TSplineInitialSymmetry.ByAxial' 노드로 정의된 대칭 축을 중심으로 대칭됩니다.

## 예제 파일

![Example](./Autodesk.DesignScript.Geometry.TSpline.TSplineInitialSymmetry.ByAxial_img.gif)
