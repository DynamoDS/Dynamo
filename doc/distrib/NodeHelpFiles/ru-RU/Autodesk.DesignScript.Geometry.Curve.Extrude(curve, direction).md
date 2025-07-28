## Подробности
`Curve.Extrude (curve, direction)` выдавливает входную кривую с помощью входного вектора для определения направления выдавливания. Длина вектора используется для определения расстояния выдавливания.

В примере ниже сначала создается объект NurbsCurve с помощью узла `NurbsCurve.ByControlPoints`, в котором входной набор точек генерируется случайным образом. Для задания компонентов X, Y и Z узла `Vector.ByCoordinates` используется блок кода. Этот вектор затем используется как входное значение направления `direction` для узла `Curve.Extrude`.
___
## Файл примера

![Curve.Extrude(curve, direction)](./Autodesk.DesignScript.Geometry.Curve.Extrude(curve,%20direction)_img.jpg)
