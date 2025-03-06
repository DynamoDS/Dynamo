## In-Depth
`TSplineFace.Info` renvoie les propriétés suivantes d'une face de T-Spline :
- `uvnFrame` : point sur la coque, vecteur U, vecteur V et vecteur normal de la face de T-Spline
- `index` : index de la face
- `valence` : nombre de sommets ou d'arêtes formant une face
- `sides` : nombre d'arêtes de chaque face de T-Spline

Dans l'exemple ci-dessous, les attributs `TSplineSurface.ByBoxCorners` et `TSplineTopology.RegularFaces` sont utilisés respectivement pour créer une T-Spline et sélectionner ses faces. `List.GetItemAtIndex` est utilisé pour choisir une face spécifique de la T-Spline et `TSplineFace.Info` est utilisé pour trouver ses propriétés.

## Exemple de fichier

![Example](./Autodesk.DesignScript.Geometry.TSpline.TSplineFace.Info_img.jpg)
