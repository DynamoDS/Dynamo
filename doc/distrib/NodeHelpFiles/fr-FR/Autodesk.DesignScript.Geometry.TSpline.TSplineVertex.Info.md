## In-Depth
`TSplineVertex.Info` renvoie les propriétés suivantes d'un sommet de T-Spline :
- `uvnFrame` : point sur la coque, vecteur U, vecteur V et vecteur normal du sommet de T-Spline
- `index` : index du sommet choisi sur la surface de T-spline
- `isStarPoint` : indique si le sommet choisi est un point d'étoile
- `isTpoint` : indique si le sommet choisi est un point T
- `isManifold` : indique si le sommet choisi est Manifold
- `valence` : nombre d'arêtes sur le sommet de T-Spline choisi
- `functionalValence` : valence fonctionnelle d'un sommet. Pour plus d'informations, reportez-vous à la documentation relative au noeud `TSplineVertex.FunctionalValence`.

Dans l'exemple ci-dessous, `TSplineSurface.ByBoxCorners` et `TSplineTopology.VertexByIndex` sont utilisés pour créer une surface de T-Spline et sélectionner ses sommets. `TSplineVertex.Info` est utilisé pour collecter les informations ci-dessus à propos d'un sommet choisi.

## Exemple de fichier

![Example](./Autodesk.DesignScript.Geometry.TSpline.TSplineVertex.Info_img.jpg)
