<!--- Autodesk.DesignScript.Geometry.TSpline.TSplineSurface.ByPlaneThreePoints --->
<!--- SFTUBFPMM3AWPUQ6E6XPGTDHXANNIVC3ZHSMIP63ZGMSHIEQMWFQ --->
## In-Depth
Узел `TSplineSurface.ByPlaneThreePoints` создает плоскую Т-сплайновую поверхность-примитив по трем входным точкам. Для создания Т-сплайновой плоскости узел использует следующие входные параметры:
— `p1`, `p2` и `p3`: три точки, определяющие положение плоскости. Первая точка принимается за начало координат плоскости.
- `minCorner` and `maxCorner`: the corners of the plane, represented as Points with X and Y values (Z coordinates will be ignored). These corners represent the extents of the output T-Spline surface if it is translated onto the XY plane. The `minCorner` and `maxCorner` points do not have to coincide with the corner vertices in 3D. For example, when a `minCorner` is set to (0,0) and `maxCorner` is (5,10), the plane width and length will be 5 and 10 respectively.
- `xSpans` and `ySpans`: number of width and length spans/divisions of the plane
- `symmetry`: whether the geometry is symmetrical with respect to its X, Y and Z axes
- `inSmoothMode`: whether the resulting geometry will appear with smooth or box mode

В приведенном ниже примере плоская Т-сплайновая поверхность создается по трем случайным точкам. Первая точка используется в качестве начала координат плоскости. Размер поверхности определяется двумя точками, используемыми в качестве значений входных параметров `minCorner` и `maxCorner`.

## Файл примера

![Example](./SFTUBFPMM3AWPUQ6E6XPGTDHXANNIVC3ZHSMIP63ZGMSHIEQMWFQ_img.jpg)
