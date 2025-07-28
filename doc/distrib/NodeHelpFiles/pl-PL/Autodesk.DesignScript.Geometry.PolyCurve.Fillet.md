## Informacje szczegółowe
Węzeł Fillet zwraca nową bryłę z zaokrąglonymi krawędziami. Wartość wejściowa edges określa, które krawędzie mają zostać zaokrąglone, natomiast wartość wejściowa odsunięcia określa promień zaokrąglenia. W poniższym przykładzie zaczynamy od sześcianu, używając domyślnych danych wejściowych. Aby uzyskać odpowiednie krawędzie sześcianu, najpierw rozbijamy go w celu uzyskania powierzchni jako listy powierzchni. Następnie za pomocą węzła Face.Edges wyodrębniamy krawędzie sześcianu. Pierwszą krawędź każdej powierzchni wyodrębniamy za pomocą węzła GetItemAtIndex. Suwak Number Slider steruje promieniem każdego zaokrąglenia.
___
## Plik przykładowy

![Fillet](./Autodesk.DesignScript.Geometry.PolyCurve.Fillet_img.jpg)

