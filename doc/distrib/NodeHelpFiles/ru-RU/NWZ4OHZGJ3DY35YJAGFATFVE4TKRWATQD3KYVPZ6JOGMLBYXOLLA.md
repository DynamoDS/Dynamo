<!--- Autodesk.DesignScript.Geometry.Curve.ExtrudeAsSolid(curve, distance) --->
<!--- NWZ4OHZGJ3DY35YJAGFATFVE4TKRWATQD3KYVPZ6JOGMLBYXOLLA --->
## Подробности
`Curve.ExtrudeAsSolid (curve, distance)` выдавливает входную замкнутую плоскую кривую на основе входящего числового значения для определения расстояния выдавливания. Направление выдавливания определяется вектором нормали плоскости, в которой лежит кривая. Этот узел закрывает концы выдавливания для создания тела.

В примере ниже сначала создается объект NurbsCurve с помощью узла `NurbsCurve.ByPoints`, в качестве входных данных которого используется набор случайных точек. Затем для выдавливания кривой в качестве тела используется узел `Curve.ExtrudeAsSolid`. Регулятор чисел используется как входное значение `distance` в узле `Curve.ExtrudeAsSolid`.
___
## Файл примера

![Curve.ExtrudeAsSolid(curve, distance)](./NWZ4OHZGJ3DY35YJAGFATFVE4TKRWATQD3KYVPZ6JOGMLBYXOLLA_img.jpg)
