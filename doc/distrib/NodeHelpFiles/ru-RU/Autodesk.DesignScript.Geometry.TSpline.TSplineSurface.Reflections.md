## Подробности
В приведенном ниже примере Т-сплайновая поверхность с добавленными отражениями проверяется с помощью узла `TSplineSurface.Reflections`, который возвращает список всех отражений, примененных к поверхности. В результате создается список двух отражений. Затем та же самая поверхность проходит через узел `TSplineSurface.RemoveReflections`, после чего снова проверяется. На этот раз узел `TSplineSurface.Reflections` возвращает ошибку, поскольку отражения были удалены.
___
## Файл примера

![TSplineSurface.Reflections](./Autodesk.DesignScript.Geometry.TSpline.TSplineSurface.Reflections_img.jpg)
