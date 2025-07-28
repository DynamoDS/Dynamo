## Podrobnosti
V níže uvedeném příkladu je vytvořen kvádr T-Spline pomocí uzlu `TSplineSurface.ByBoxLengths` se zadaným počátkem, šířkou, délkou, výškou, rozpětími a symetrií.
Pomocí uzlu `EdgeByIndex` se poté vybere hrana ze seznamu hran v generovaném povrchu. Vybraná hrana se poté posune podél sousedních hran pomocí uzlu `TSplineSurface.SlideEdges` a její symetrické protějšky budou následovat.
___
## Vzorový soubor

![TSplineTopology.EdgeByIndex](./Autodesk.DesignScript.Geometry.TSpline.TSplineTopology.EdgeByIndex_img.jpg)
