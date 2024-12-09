## Informacje szczegółowe
Węzeł UV Parameter At Point wyszukuje położenie UV powierzchni (surface) w punkcie wejściowym (point) na powierzchni. Jeśli punkt wejściowy nie znajduje się na danej powierzchni, węzeł znajdzie punkt na powierzchni najbliższy punktowi wejściowemu. W poniższym przykładzie najpierw tworzymy powierzchnię za pomocą węzła BySweep2Rails. Następnie za pomocą węzła Code Block określamy punkt, w którym należy znaleźć parametr UV. Punkt nie znajduje się na powierzchni, więc węzeł używa najbliższego punktu na powierzchni jako położenia, w którym należy znaleźć parametr UV.
___
## Plik przykładowy

![UVParameterAtPoint](./Autodesk.DesignScript.Geometry.Surface.UVParameterAtPoint_img.jpg)

