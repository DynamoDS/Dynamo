<!--- Autodesk.DesignScript.Geometry.Curve.SweepAsSolid(curve, path, cutEndOff) --->
<!--- LUVHU25JWECNEBKIBZFH6N5EUAM42XM3BSEOTMCI3TQDNS5EKLXA --->
## Подробности
`Curve.SweepAsSolid` создает тело путем сдвига входной замкнутой кривой профиля вдоль заданной траектории.

В примере ниже в качестве базовой кривой профиля используется прямоугольник. Траектория создается с помощью косинусной функции с последовательностью углов для изменения координат X набора точек. Точки используются в качестве входных данных для узла `NurbsCurve.ByPoints`. Затем мы создаем тело, сдвигая прямоугольник по созданной косинусоиде с помощью узла `Curve.SweepAsSolid`.
___
## Файл примера

![Curve.SweepAsSolid(curve, path, cutEndOff)](./LUVHU25JWECNEBKIBZFH6N5EUAM42XM3BSEOTMCI3TQDNS5EKLXA_img.jpg)
