## In-Depth
`TSplineVertex.IsTPoint` permet de déterminer si un sommet est un point T. Les points T sont des sommets situés à la fin de lignes partielles de points de contrôle.

Dans l'exemple ci-dessous, `TSplineSurface.SubdiviserFaces` est utilisé sur une primitive de boîte de T-Spline pour illustrer une des multiples modalités d'ajout de Points T à une surface. Le noeud `TSplineVertex.IsTPoint` est utilisé pour confirmer qu'un sommet au niveau d'un index est un Point T. Pour mieux visualiser la position des Points T, les noeuds `TSplineVertex.UVNFrame` et `TSplineUVNFrame.Position` sont utilisés.



## Exemple de fichier

![Example](./Autodesk.DesignScript.Geometry.TSpline.TSplineVertex.IsTPoint_img.jpg)
