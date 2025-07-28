<!--- Autodesk.DesignScript.Geometry.TSpline.TSplineSurface.ByPlaneLineAndPoint --->
<!--- SFB4J46343LP6YKDRW2FPILSS6UXITLDXWQKYJRD6LWHQJY2IYOA --->
## In-Depth
Узел `TSplineSurface.ByPlaneLineAndPoint` создает плоскую Т-сплайновую поверхность-примитив по отрезку и точке. Итоговая Т-сплайновая поверхность является плоскостью. Для создания Т-сплайновой плоскости узел использует следующие входные параметры:
— `line` и `point`: входные параметры, необходимые для определения ориентации и положения плоскости.
- `minCorner` and `maxCorner`: the corners of the plane, represented as Points with X and Y values (Z coordinates will be ignored). These corners represent the extents of the output T-Spline surface if it is translated onto the XY plane. The `minCorner` and `maxCorner` points do not have to coincide with the corner vertices in 3D. For example, when a `minCorner` is set to (0,0) and `maxCorner` is (5,10), the plane width and length will be 5 and 10 respectively.
- `xSpans` and `ySpans`: number of width and length spans/divisions of the plane
- `symmetry`: whether the geometry is symmetrical with respect to its X, Y and Z axes
- `inSmoothMode`: whether the resulting geometry will appear with smooth or box mode

В приведенном ниже примере плоская Т-сплайновая поверхность создается по заданному отрезку и плоскости. Размер поверхности определяется двумя точками, используемыми в качестве значений входных параметров `minCorner` и `maxCorner`.

## Файл примера

![Example](./SFB4J46343LP6YKDRW2FPILSS6UXITLDXWQKYJRD6LWHQJY2IYOA_img.jpg)
