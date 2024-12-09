<!--- Autodesk.DesignScript.Geometry.TSpline.TSplineSurface.DuplicateFaces --->
<!--- QVBZTZWGLGK2PKP6QSZJI7UBI2Y5Z7HF4ZG7JKETOZCBLOF5IIPA --->
## Подробности
Узел `TSplineSurface.DuplicateFaces` создает новую Т-сплайновую поверхность, состоящую только из выбранных скопированных граней.

В приведенном ниже примере Т-сплайновая поверхность создается с помощью узла `TSplineSurface.ByRevolve`, который использует NURBS-кривую в качестве профиля.
Затем с помощью узла `TSplineTopology.FaceByIndex` выбирается набор граней на поверхности. Эти грани копируются с помощью узла `TSplineSurface.DuplicateFaces`, а итоговая поверхность смещается в сторону для удобства визуализации.
___
## Файл примера

![TSplineSurface.DuplicateFaces](./QVBZTZWGLGK2PKP6QSZJI7UBI2Y5Z7HF4ZG7JKETOZCBLOF5IIPA_img.jpg)
