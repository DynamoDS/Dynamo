## Подробности
Узел `TSplineSurface.FlattenVertices(vertices, parallelPlane)` изменяет положение управляющих точек для указанного набора вершин путем их выравнивания по плоскости `parallelPlane`, задаваемой в качестве входного значения.

В приведенном ниже примере вершины плоской Т-сплайновой поверхности смещаются с помощью узлов `TsplineTopology.VertexByIndex` и `TSplineSurface.MoveVertices`. Затем поверхность смещается в сторону для удобства предварительного просмотра и используется в качестве входного параметра для узла `TSplineSurface.FlattenVertices(vertices, parallelPlane)`. В результате создается новая поверхность с выбранными вершинами, лежащими точно на заданной плоскости.
___
## Файл примера

![TSplineSurface.FlattenVertices](./XGSWLBVZ2TGT6X7FZRUWJDGDSBL7JWKDBQYWEJWO7VZOJPJI7OWQ_img.jpg)
