## Informacje szczegółowe
Węzeł Chamfer zwraca nową bryłę z fazowanymi krawędziami. Pozycja wejściowa krawędzi (edges) określa, które krawędzie mają zostać sfazowane, natomiast pozycja wejściowa odsunięcia (offset) określa zakres fazy. W poniższym przykładzie zaczynamy od sześcianu utworzonego przy użyciu domyślnych danych wejściowych. Aby uzyskać odpowiednie krawędzie sześcianu, najpierw rozbijamy go w celu uzyskania listy jego powierzchni. Następnie za pomocą węzła Face.Edges wyodrębniamy krawędzie sześcianu. Wyodrębniamy pierwszą krawędź każdej powierzchni za pomocą węzła GetItemAtIndex. Suwak Number Slider steruje odległością odsunięcia dla fazy.
___
## Plik przykładowy

![Chamfer](./Autodesk.DesignScript.Geometry.Solid.Chamfer_img.jpg)

