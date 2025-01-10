<!--- Autodesk.DesignScript.Geometry.TSpline.TSplineSurface.CompressIndexes --->
<!--- ARIV6OQ22ACATWAIKGM7OHNEJS2TQUOKUSEU6UNX6EAAVSJIMK3A --->
## Подробности
Узел `TSplineSurface.CompressIndexes` удаляет пропуски в номерах индексов ребер, вершин или граней Т-сплайновой поверхности, образующиеся в результате различных операций, таких как удаление грани. Порядок индексов сохраняется.

В приведенном ниже примере ряд граней удаляется из поверхности-примитива тетрагональной сферы, что влияет на индексы ребер формы. Узел `TSplineSurface.CompressIndexes` используется для восстановления индексов ребер формы, благодаря чему появляется возможность выбрать ребро с индексом 1.

## Файл примера

![Example](./ARIV6OQ22ACATWAIKGM7OHNEJS2TQUOKUSEU6UNX6EAAVSJIMK3A_img.jpg)
