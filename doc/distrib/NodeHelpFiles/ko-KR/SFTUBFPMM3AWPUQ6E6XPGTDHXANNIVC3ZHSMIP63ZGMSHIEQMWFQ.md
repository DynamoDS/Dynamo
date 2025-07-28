<!--- Autodesk.DesignScript.Geometry.TSpline.TSplineSurface.ByPlaneThreePoints --->
<!--- SFTUBFPMM3AWPUQ6E6XPGTDHXANNIVC3ZHSMIP63ZGMSHIEQMWFQ --->
## In-Depth
`TSplineSurface.ByPlaneThreePoints`는 3개의 점을 입력으로 사용하여 T-Spline 원형 평면 표면을 생성합니다. T-Spline 평면을 작성하려면 노드가 다음 입력을 사용합니다.
`p1`, `p2` 및 `p3`: 평면의 위치를 정의하는 3개의 점. 첫 번째 점은 평면의 원점으로 간주됩니다.
- `minCorner` and `maxCorner`: the corners of the plane, represented as Points with X and Y values (Z coordinates will be ignored). These corners represent the extents of the output T-Spline surface if it is translated onto the XY plane. The `minCorner` and `maxCorner` points do not have to coincide with the corner vertices in 3D. For example, when a `minCorner` is set to (0,0) and `maxCorner` is (5,10), the plane width and length will be 5 and 10 respectively.
- `xSpans` and `ySpans`: number of width and length spans/divisions of the plane
- `symmetry`: whether the geometry is symmetrical with respect to its X, Y and Z axes
- `inSmoothMode`: whether the resulting geometry will appear with smooth or box mode

아래 예에서는 T-Spline 평면형 표면이 무작위로 생성된 3개의 점으로 작성됩니다. 첫 번째 점은 평면의 원점입니다. 표면의 크기는 `minCorner` 및 `maxCorner` 입력으로 사용되는 두 점으로 제어됩니다.

## 예제 파일

![Example](./SFTUBFPMM3AWPUQ6E6XPGTDHXANNIVC3ZHSMIP63ZGMSHIEQMWFQ_img.jpg)
