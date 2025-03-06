<!--- Autodesk.DesignScript.Geometry.Curve.ExtrudeAsSolid(curve, direction, distance) --->
<!--- EXQDCVFI3OT5SKR7TAAZHHPRQTFTGPSESCN2SXOJLSORL2ATIOCA --->
## Подробности
Curve.ExtrudeAsSolid (direction, distance) выдавливает входную замкнутую плоскую кривую, используя входной вектор для определения направления выдавливания. Для определения расстояния выдавливания используется отдельное входное значение `distance`. Этот узел закрывает концы выдавливания для создания тела.

В примере ниже сначала создается объект NurbsCurve с помощью узла `NurbsCurve.ByPoints`, в котором набор точек генерируется случайным образом. Затем для указания компонентов X, Y, Z узла `Vector.ByCoordinates` используется значение `code block`. Этот вектор потом используется как входное направление в узле `Curve.ExtrudeAsSolid`, а для управления входным значением `distance` используется регулятор чисел.
___
## Файл примера

![Curve.ExtrudeAsSolid(direction, distance)](./EXQDCVFI3OT5SKR7TAAZHHPRQTFTGPSESCN2SXOJLSORL2ATIOCA_img.jpg)
