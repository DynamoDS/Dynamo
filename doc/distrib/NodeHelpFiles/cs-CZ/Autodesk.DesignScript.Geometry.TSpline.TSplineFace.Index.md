## In-Depth
Uzel `TSplineFace.Index` vrací index plochy na povrchu T-Spline. Vezměte na vědomí, že v topologii povrchu T-Spline se indexy plochy, hrany a vrcholu nemusí nutně shodovat s pořadovým číslem položky v seznamu. Vyřešte tento problém pomocí uzlu `TSplineSurface.CompressIndices`.

V níže uvedeném příkladu se pomocí uzlu `TSplineFace.Index` zobrazují indexy všech běžných ploch povrchu T-Spline.

## Vzorový soubor

![Example](./Autodesk.DesignScript.Geometry.TSpline.TSplineFace.Index_img.jpg)
