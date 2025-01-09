## Informacje szczegółowe
Powierzchnia zamknięta to taka, która tworzy pełny kształt bez otworów ani obwiedni.
W poniższym przykładzie sfera T-splajn wygenerowana za pomocą węzła `TSplineSurface.BySphereCenterPointRadius` zostaje poddana inspekcji przy użyciu węzła `TSplineSurface.IsClosed`, aby sprawdzić, czy jest otwarta — wynik jest negatywny. Jest tak dlatego, że sfery T-splajn, mimo iż wyglądają na zamknięte, są otwarte na biegunach, gdzie w jednym punkcie nałożonych na siebie jest wiele krawędzi i wierzchołków.

Przerwy w sferze T-splajn zostają następnie wypełnione przy użyciu węzła `TSplineSurface.FillHole`, co powoduje powstanie niewielkiego odkształcenia w miejscu wypełnienia powierzchni. Po ponownym sprawdzeniu za pomocą węzła `TSplineSurface.IsClosed` wynik jest pozytywny, co oznacza, że sfera jest teraz zamknięta.
___
## Plik przykładowy

![TSplineSurface.IsClosed](./Autodesk.DesignScript.Geometry.TSpline.TSplineSurface.IsClosed_img.jpg)
