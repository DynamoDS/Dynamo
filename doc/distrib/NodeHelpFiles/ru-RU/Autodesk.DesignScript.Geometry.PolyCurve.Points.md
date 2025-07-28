## Подробности
`PolyCurve.Points` возвращает начальную точку первой составной кривой и конечные точки всех остальных составных кривых. Узел не возвращает повторяющиеся точки для замкнутых объектов PolyCurve.

В примере ниже узел `Polygon.RegularPolygon` расчленяется на список кривых, а затем снова объединяется в объект PolyCurve. Точки PolyCurve затем возвращаются с помощью узла `PolyCurve.Points`.
___
## Файл примера

![PolyCurve.Points](./Autodesk.DesignScript.Geometry.PolyCurve.Points_img.jpg)
