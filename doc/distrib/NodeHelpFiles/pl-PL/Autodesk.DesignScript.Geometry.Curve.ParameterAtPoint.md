## Informacje szczegółowe
Węzeł Parameter at Point zwraca wartość parametru określonego punktu (point) na krzywej (curve). Jeśli punkt wejściowy nie znajduje się na krzywej, węzeł Parameter at Point zwraca parametr punktu na krzywej znajdujący się najbliżej punktu wejściowego. W poniższym przykładzie najpierw tworzymy krzywą Nurbs za pomocą węzła ByControlPoints na podstawie zestawu losowo wygenerowanych punktów. Za pomocą węzła Code Block jest tworzony dodatkowy pojedynczy punkt, aby określić współrzędne x i y. Węzeł ParameterAtPoint zwraca parametr wzdłuż krzywej w punkcie najbliższym punktowi wejściowemu.
___
## Plik przykładowy

![ParameterAtPoint](./Autodesk.DesignScript.Geometry.Curve.ParameterAtPoint_img.jpg)

