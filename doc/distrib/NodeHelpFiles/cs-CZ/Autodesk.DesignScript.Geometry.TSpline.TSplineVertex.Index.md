## In-Depth
Uzel `TSplineVertex.Index` vrací číslo indexu vybraného vrcholu na povrchu T-Spline. Vezměte na vědomí, že v topologii povrchu T-Spline se indexy plochy, hrany a vrcholu nemusí nutně shodovat s pořadovým číslem položky v seznamu. Vyřešte tento problém pomocí uzlu `TSplineSurface.CompressIndices`.

V níže uvedeném příkladu se u základního útvaru T-Spline ve tvaru kvádru použije uzel `TSplineTopology.StarPointVertices`. Pomocí uzlu `TSplineVertex.Index` se poté zadávají dotazy na indexy vrcholů cípů hvězdy a uzel `TSplineTopology.VertexByIndex` vrací vybrané vrcholi k dalším úpravám.

## Vzorový soubor

![Example](./Autodesk.DesignScript.Geometry.TSpline.TSplineVertex.Index_img.jpg)
