<!--- Autodesk.DesignScript.Geometry.Curve.ExtrudeAsSolid(curve, direction) --->
<!--- 32PIZL43K2RTMXYNALUOXTTTTLRY2XQHUK22D2A7KI7NAA5JTXBA --->
## Подробности
`Curve.ExtrudeAsSolid (curve, direction)` выдавливает входную замкнутую плоскую кривую с помощью входного вектора для определения направления выдавливания. Длина вектора используется для определения расстояния выдавливания. Этот узел закрывает концы выдавливания для создания тела.

В примере ниже сначала создается объект NurbsCurve с помощью узла `NurbsCurve.ByPoints`, в котором набор точек генерируется случайным образом. Для задания компонентов X, Y и Z узла `Vector.ByCoordinates` используется блок кода. Затем этот вектор используется как входное значение направления в узле `Curve.ExtrudeAsSolid`.
___
## Файл примера

![Curve.ExtrudeAsSolid(curve, direction)](./32PIZL43K2RTMXYNALUOXTTTTLRY2XQHUK22D2A7KI7NAA5JTXBA_img.jpg)
