## Podrobnosti
V níže uvedeném příkladu je vytvořen povrch T-Spline vysunutím křivky NURBS. Pomocí uzlu `TSplineTopology.EdgeByIndex` je vybráno šest jeho hran – tři na každé straně tvaru. Dvě sady hran spolu s povrchem jsou předány do uzlu `TSplineSurface.MergeEdges`. Pořadí skupin hran ovlivňuje tvar – první skupina hran je posunuta tak, aby odpovídala druhé skupině, která zůstává na stejném místě. Vstup `insertCreases` přidá možnost vyostření švu podél sloučených hran. Výsledek operace sloučení je přesunut stranu kvůli lepšímu náhledu.
___
## Vzorový soubor

![TSplineSurface.MergeEdges](./Autodesk.DesignScript.Geometry.TSpline.TSplineSurface.MergeEdges_img.gif)
