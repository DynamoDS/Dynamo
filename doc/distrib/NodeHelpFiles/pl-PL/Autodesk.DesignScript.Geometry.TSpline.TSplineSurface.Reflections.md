## Informacje szczegółowe
W poniższym przykładzie przy użyciu węzła `TSplineSurface.Reflections` zostaje zbadana powierzchnia T-splajn z dodanymi odbiciami, co powoduje zwrócenie listy wszystkich odbić zastosowanych do powierzchni. Wynikiem jest lista dwóch odbić. Ta sama powierzchnia przechodzi następnie przez węzeł `TSplineSurface.RemoveReflections` i zostaje sprawdzona ponownie. Tym razem węzeł `TSplineSurface.Reflections` zwraca błąd, ponieważ odbicia zostały usunięte.
___
## Plik przykładowy

![TSplineSurface.Reflections](./Autodesk.DesignScript.Geometry.TSpline.TSplineSurface.Reflections_img.jpg)
