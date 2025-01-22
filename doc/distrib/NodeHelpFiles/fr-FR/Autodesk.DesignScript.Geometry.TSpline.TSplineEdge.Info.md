## In-Depth
`TSplineEdge.Info` renvoie les propriétés suivantes d'une arête de surface de T-Spline :
- `uvnFrame` : point sur la coque, vecteur U, vecteur V et vecteur normal de l'arête de T-Spline
- `index` : index de l'arête
- `isBorder` : indique si l'arête choisie est une bordure de surface de T-Spline
- `isManifold` : indique si l'arête choisie est manifold

Dans l'exemple ci-dessous, `TSplineTopology.DecomposedEdges` est utilisé pour obtenir une liste de toutes les arêtes d'une surface primitive de cylindre de T-Spline et `TSplineEdge.Info` est utilisé pour examiner leurs propriétés.


## Exemple de fichier

![Example](./Autodesk.DesignScript.Geometry.TSpline.TSplineEdge.Info_img.jpg)
