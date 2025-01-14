## Podrobnosti
V níže uvedeném příkladu je vrchol povrchu T-Spline získán pomocí uzlu `TSplineTopology.VertexByIndex`. Vrchol se poté použije jako vstup pro uzel `TSplineSurface.MoveVertices`. Vrchol se posune ve směru určeném vstupem `vector`. Vstup `onSurface` buď bere v úvahu pro pohyb povrch, když je nastaven na hodnotu `True`, nebo řídicí body, pokud je nastaven na hodnotu `False`.
___
## Vzorový soubor

![TSplineSurface.MoveVertices](./Autodesk.DesignScript.Geometry.TSpline.TSplineSurface.MoveVertices_img.jpg)
