<!--- Autodesk.DesignScript.Geometry.TSpline.TSplineSurface.ByPlaneOriginNormalXAxis --->
<!--- IPJR2I55VFC67ZT5OYCCOOAEDH2AHY3CSCZKVXKIXALKV7IBDERA --->
## In-Depth
Узел `TSplineSurface.ByPlaneOriginNormalXAxis` создает плоскую Т-сплайновую поверхность-примитив с использованием начала координат, вектора нормали и направления вектора оси X плоскости. Для создания Т-сплайновой плоскости узел использует следующие входные параметры:
- `origin`: a point defining the origin of the plane.
- `normal`: a vector specifying the normal direction of the created plane.
— `xAxis`: вектор, определяющий направление оси X и позволяющий точнее задать ориентацию созданной плоскости.
- `minCorner` and `maxCorner`: the corners of the plane, represented as Points with X and Y values (Z coordinates will be ignored). These corners represent the extents of the output T-Spline surface if it is translated onto the XY plane. The `minCorner` and `maxCorner` points do not have to coincide with the corner vertices in 3D. For example, when a `minCorner` is set to (0,0) and `maxCorner` is (5,10), the plane width and length will be 5 and 10 respectively.
- `xSpans` and `ySpans`: number of width and length spans/divisions of the plane
- `symmetry`: whether the geometry is symmetrical with respect to its X, Y and Z axes
- `inSmoothMode`: whether the resulting geometry will appear with smooth or box mode

В приведенном ниже примере плоская Т-сплайновая поверхность создается с использованием заданного начала координат и нормали, которая представляет собой вектор оси X. В качестве значения входного параметра `xAxis` задана ось Z. Размер поверхности определяется двумя точками, используемыми в качестве значений входных параметров `minCorner` и `maxCorner`.

## Файл примера

![Example](./IPJR2I55VFC67ZT5OYCCOOAEDH2AHY3CSCZKVXKIXALKV7IBDERA_img.jpg)
