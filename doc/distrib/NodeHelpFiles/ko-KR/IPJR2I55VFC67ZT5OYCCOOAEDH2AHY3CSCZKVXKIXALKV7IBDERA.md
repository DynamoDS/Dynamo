<!--- Autodesk.DesignScript.Geometry.TSpline.TSplineSurface.ByPlaneOriginNormalXAxis --->
<!--- IPJR2I55VFC67ZT5OYCCOOAEDH2AHY3CSCZKVXKIXALKV7IBDERA --->
## In-Depth
`TSplineSurface.ByPlaneOriginNormalXAxis`는 원점, 법선 벡터, 평면의 X축의 벡터 방향을 사용하여 T-Spline 원형 평면 표면을 생성합니다. T-Spline 평면을 작성하려면 노드가 다음 입력을 사용합니다.
- `origin`: a point defining the origin of the plane.
- `normal`: a vector specifying the normal direction of the created plane.
`xAxis` : X축의 방향을 정의하는 벡터이며, 이를 통해 작성된 평면의 방향을 더 잘 제어할 수 있습니다.
- `minCorner` and `maxCorner`: the corners of the plane, represented as Points with X and Y values (Z coordinates will be ignored). These corners represent the extents of the output T-Spline surface if it is translated onto the XY plane. The `minCorner` and `maxCorner` points do not have to coincide with the corner vertices in 3D. For example, when a `minCorner` is set to (0,0) and `maxCorner` is (5,10), the plane width and length will be 5 and 10 respectively.
- `xSpans` and `ySpans`: number of width and length spans/divisions of the plane
- `symmetry`: whether the geometry is symmetrical with respect to its X, Y and Z axes
- `inSmoothMode`: whether the resulting geometry will appear with smooth or box mode

아래 예에서는 T-Spline 평면형 표면이 제공된 원점과 X축의 벡터인 법선을 사용하여 작성됩니다. `xAxis` 입력은 Z축으로 설정됩니다. 표면의 크기는 `minCorner` 및 `maxCorner` 입력으로 사용되는 두 점으로 제어됩니다.

## 예제 파일

![Example](./IPJR2I55VFC67ZT5OYCCOOAEDH2AHY3CSCZKVXKIXALKV7IBDERA_img.jpg)
