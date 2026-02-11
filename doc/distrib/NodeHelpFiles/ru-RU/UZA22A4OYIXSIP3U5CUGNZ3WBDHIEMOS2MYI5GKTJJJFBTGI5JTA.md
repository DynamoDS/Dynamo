<!--- Autodesk.DesignScript.Geometry.TSpline.TSplineSurface.WeldCoincidentVertices --->
<!--- UZA22A4OYIXSIP3U5CUGNZ3WBDHIEMOS2MYI5GKTJJJFBTGI5JTA --->
## Подробности
В приведенном ниже примере две половины Т-сплайновой поверхности объединяются в одну с помощью узла `TSplineSurface.ByCombinedTSplineSurfaces`. Вершины вдоль плоскости отражения перекрываются, что становится заметно при перемещении одной из вершин с помощью узла `TSplineSurface.MoveVertices`. Чтобы это исправить, выполняется объединение с помощью узла `TSplineSurface.WeldCoincidentVertices`. Теперь результат перемещения вершины отличается (смещен в сторону для удобства предварительного просмотра).
___
## Файл примера

![TSplineSurface.WeldCoincidentVertices](./UZA22A4OYIXSIP3U5CUGNZ3WBDHIEMOS2MYI5GKTJJJFBTGI5JTA_img.jpg)
