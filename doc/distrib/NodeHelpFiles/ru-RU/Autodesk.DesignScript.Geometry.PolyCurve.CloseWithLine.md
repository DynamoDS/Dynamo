## Подробности
CloseWithLine позволяет добавить прямую линию между начальной и конечной точками незамкнутого объекта PolyCurve. Этот узел возвращает новую сложную кривую, включающую в себя добавленную прямую. В примере ниже создается набор случайных точек, а затем создается незамкнутый объект PolyCurve с использованием узла PolyCurve.ByPoints с входным элементом connectLastToFirst, для которого задано значение False. При вставке этого объекта PolyCurve в узел CloseWithLine создается новая замкнутая сложная кривая (в данном случае это аналогично использованию входного значения True для параметра connectLastToFirst в узле PolyCurve.ByPoints).
___
## Файл примера

![CloseWithLine](./Autodesk.DesignScript.Geometry.PolyCurve.CloseWithLine_img.jpg)

