## In-Depth
Pokud je vstupní hrana T-spline hranicí, uzel `TSplineEdge.IsBorder` vrátí hodnotu `True`.

V níže uvedeném příkladu jsou prozkoumány hrany dvou povrchů T-Spline. Povrchy jsou válec a jeho zesílená verze. Chcete-li vybrat všechny hrany, použijte uzly `TSplineTopology.EdgeByIndex` v obou případech se vstupem indexů – rozsah celých čísel od 0 do n, kde n je počet hran poskytnutých uzlem `TSplineTopology.EdgesCount`. Toto je alternativa k přímému výběru hran pomocí uzlu `TSplineTopology.DecomposedEdges`. V případě zesíleného válce se navíc pomocí uzlu `TSplineSurface.CompressIndices` změní pořadí indexů hran.
Uzel `TSplineEdge.IsBorder` slouží ke kontrole, které z hran jsou okrajové hrany. Pozice okrajových hran plochého válce je zvýrazněna pomocí uzlů `TSplineEdge.UVNFrame` a `TSplineUVNFrame.Position`. Zesílený válec nemá žádné okrajové hrany.

## Vzorový soubor

![Example](./Autodesk.DesignScript.Geometry.TSpline.TSplineEdge.IsBorder_img.jpg)
