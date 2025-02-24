## Подробности
В приведенном ниже примере плоская Т-сплайновая поверхность создается с помощью узла `TSplineSurface.ByPlaneOriginNormal`, а набор ее граней выбирается и разделяется. Затем эти грани симметрично выдавливаются с помощью узла `TSplineSurface.ExtrudeFaces` в соответствии с заданным направлением (в данном случае это вектор нормали UVN граней) и количеством пролетов. Итоговые ребра смещаются в заданном направлении.
___
## Файл примера

![TSplineSurface.ExtrudeFaces](./Autodesk.DesignScript.Geometry.TSpline.TSplineSurface.ExtrudeFaces_img.jpg)
