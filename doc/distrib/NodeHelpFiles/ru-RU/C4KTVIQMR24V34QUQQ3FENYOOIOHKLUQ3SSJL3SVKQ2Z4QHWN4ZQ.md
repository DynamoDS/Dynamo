<!--- Autodesk.DesignScript.Geometry.TSpline.TSplineSurface.ByNurbsSurfaceUniform --->
<!--- C4KTVIQMR24V34QUQQ3FENYOOIOHKLUQ3SSJL3SVKQ2Z4QHWN4ZQ --->
## Подробности
В приведенном ниже примере поверхность NURBS 3-го порядка преобразуется в Т-сплайновую поверхность с помощью узла `TSplineSurface.ByNurbsSurfaceUniform`. Входная поверхность NURBS преобразуется путем размещения однородных узлов с одинаковыми параметрическими интервалами или интервалами, равными заданной длине дуги, в зависимости от соответствующих входных параметров `uUseArcLen` и `vUseArcLen`, после чего аппроксимируется поверхностью NURBS 3-го порядка. Выходной T-сплайн разделяется на отрезки в соответствии со значениями параметров `uSpan` и `vSpan` в направлениях U и V.

## Файл примера

![Example](./C4KTVIQMR24V34QUQQ3FENYOOIOHKLUQ3SSJL3SVKQ2Z4QHWN4ZQ_img.jpg)
