## In-Depth
Snímek plochy UVNFrame nabízí užitečné informace o pozici a orientaci plochy tím, že vrací normálový vektor a směry UV.
V níže uvedeném příkladu se pomocí uzlu `TSplineFace.UVNFrame` vizualizuje rozložení ploch na základní čtyřúhelníkové kouli. Pomocí uzlu `TSplineTopology.DecomposedFaces` se zadá dotaz na všechny plochy a poté se pomocí uzlu `TSplineFace.UVNFrame` získají pozice těžišť ploch jako body. Body jsou vizualizovány pomocí uzlu `TSplineUVNFrame.Position`. Popisky je možné zobrazit v náhledu pozadí povolením možnosti Zobrazit popisky v místní nabídce uzlu.

## Vzorový soubor

![Example](./Autodesk.DesignScript.Geometry.TSpline.TSplineFace.UVNFrame_img.jpg)
