## In-Depth
Uzel `TSplineEdge.Info` vrátí následující vlastnosti hrany povrchu T-Spline:
- `uvnFrame`: bod na trupu, vektor U, vektor V a normálový vektor hrany T-Spline
- `index`: index hrany
- `isBorder`: určuje, zda je vybraná hrana hranicí povrchu T-Spline
- `isManifold`: určuje, zda je vybraná hrana rozložená

V níže uvedeném příkladu se pomocí uzlu `TSplineTopology.DecomposedEdges` získá seznam všech hran základního povrchu válce T-Spline a pomocí uzlu `TSplineEdge.Info` se prozkoumají jejich vlastnosti.


## Vzorový soubor

![Example](./Autodesk.DesignScript.Geometry.TSpline.TSplineEdge.Info_img.jpg)
