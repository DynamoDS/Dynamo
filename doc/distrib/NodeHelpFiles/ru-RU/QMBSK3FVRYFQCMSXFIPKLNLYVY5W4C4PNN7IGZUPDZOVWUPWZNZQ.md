<!--- Autodesk.DesignScript.Geometry.TSpline.TSplineSurface.ByPlaneBestFitThroughPoints --->
<!--- QMBSK3FVRYFQCMSXFIPKLNLYVY5W4C4PNN7IGZUPDZOVWUPWZNZQ --->
## In-Depth
Узел `TSplineSurface.ByPlaneBestFitThroughPoints` создает плоскую Т-сплайновую поверхность-примитив на основе списка точек. Для создания Т-сплайновой плоскости узел использует следующие входные параметры:
— `points`: набор точек для определения ориентации плоскости и начала координат. Когда входные точки лежат в разных плоскостях, ориентация плоскости определяется путем оптимального вписывания. Для создания поверхности требуется по крайней мере три точки.
— `minCorner` и `maxCorner`: углы плоскости, представленные точками со значениями X и Y (координаты Z игнорируются). Эти углы представляют границы выходной Т-сплайновой поверхности, если она перемещается на плоскость XY. Точки`minCorner` и `maxCorner` не должны совпадать с вершинами углов в 3D.
- `xSpans` and `ySpans`: number of width and length spans/divisions of the plane
- `symmetry`: whether the geometry is symmetrical with respect to its X, Y and Z axes
- `inSmoothMode`: whether the resulting geometry will appear with smooth or box mode

В приведенном ниже примере плоская Т-сплайновая поверхность создается с использованием списка случайных точек. Размер поверхности определяется двумя точками, используемыми в качестве значений входных параметров `minCorner` и `maxCorner`.

## Файл примера

![Example](./QMBSK3FVRYFQCMSXFIPKLNLYVY5W4C4PNN7IGZUPDZOVWUPWZNZQ_img.jpg)
