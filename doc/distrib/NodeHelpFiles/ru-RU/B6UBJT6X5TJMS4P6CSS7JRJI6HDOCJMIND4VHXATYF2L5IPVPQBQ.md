<!--- Autodesk.DesignScript.Geometry.TSpline.TSplineSurface.RemoveReflections --->
<!--- B6UBJT6X5TJMS4P6CSS7JRJI6HDOCJMIND4VHXATYF2L5IPVPQBQ --->
## In-Depth
Узел `TSplineSurface.RemoveReflections` удаляет отражения из входных данных `tSplineSurface`. Удаление отражений не изменяет форму, однако разрушает зависимость между отраженными частями геометрии, позволяя редактировать их независимо друг от друга.

В приведенном ниже примере сначала создается Т-сплайновая поверхность путем применения осевых и радиальных отражений. Затем поверхность передается в узел `TSplineSurface.RemoveReflections`, который удаляет отражения. Чтобы продемонстрировать, как это влияет на последующие изменения, одна из вершин перемещается с помощью узла `TSplineSurface.MoveVertex`. Из-за удаления отражений с поверхности изменяется только одна вершина.

## Файл примера

![Example](./B6UBJT6X5TJMS4P6CSS7JRJI6HDOCJMIND4VHXATYF2L5IPVPQBQ_img.jpg)
