<!--- Autodesk.DesignScript.Geometry.TSpline.TSplineSurface.UnweldVertices --->
<!--- D6ESVRBW5SSM4F7J65W7OKR5DTKJAFW5F5AP23FRRR3MGXKIFVOQ --->
## In-Depth
Аналогично узлу `TSplineSurface.UnweldEdges`, этот узел выполняет операцию разъединения для множества вершин. В результате разъединяются все ребра, соединенные в выбранной вершине. В отличие от операции удаления сгиба, которая создает резкий переход вокруг вершины с сохранением соединения, операция разъединения создает разрыв.

В приведенном ниже примере одна из выбранных вершин Т-сплайновой плоскости отсоединяется с помощью узла `TSplineSurface.UnweldVertices`. Разрыв формируется вдоль ребер, окружающих выбранную вершину, что демонстрируется путем перемещения вершины вверх с помощью узла `TSplineSurface.MoveVertices`.

## Файл примера

![Example](./D6ESVRBW5SSM4F7J65W7OKR5DTKJAFW5F5AP23FRRR3MGXKIFVOQ_img.jpg)
