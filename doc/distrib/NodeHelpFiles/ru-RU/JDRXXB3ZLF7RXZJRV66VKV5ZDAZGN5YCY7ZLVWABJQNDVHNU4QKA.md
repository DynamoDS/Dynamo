<!--- Autodesk.DesignScript.Geometry.TSpline.TSplineSurface.ByPlaneOriginXAxisYAxis --->
<!--- JDRXXB3ZLF7RXZJRV66VKV5ZDAZGN5YCY7ZLVWABJQNDVHNU4QKA --->
## In-Depth
Узел `TSplineSurface.ByPlaneOriginXAxisYAxis` создает плоскую Т-сплайновую поверхность-примитив с использованием начала координат и двух векторов, представляющих оси X и Y плоскости. Для создания Т-сплайновой плоскости узел использует следующие входные параметры:
- `origin`: a point defining the origin of the plane.
— `xAxis` и `yAxis`: векторы, определяющие направление осей X и Y созданной плоскости.
- `minCorner` and `maxCorner`: the corners of the plane, represented as Points with X and Y values (Z coordinates will be ignored). These corners represent the extents of the output T-Spline surface if it is translated onto the XY plane. The `minCorner` and `maxCorner` points do not have to coincide with the corner vertices in 3D. For example, when a `minCorner` is set to (0,0) and `maxCorner` is (5,10), the plane width and length will be 5 and 10 respectively.
- `xSpans` and `ySpans`: number of width and length spans/divisions of the plane
- `symmetry`: whether the geometry is symmetrical with respect to its X, Y and Z axes
- `inSmoothMode`: whether the resulting geometry will appear with smooth or box mode

В приведенном ниже примере создается плоская T-сплайновая поверхность с использованием заданного начала координат и двух векторов, определяющих направления X и Y. Размер поверхности определяется двумя точками, используемыми в качестве значений входных параметров `minCorner` и `maxCorner`.

## Файл примера

![Example](./JDRXXB3ZLF7RXZJRV66VKV5ZDAZGN5YCY7ZLVWABJQNDVHNU4QKA_img.jpg)
