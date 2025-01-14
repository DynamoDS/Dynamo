## In-Depth
`TSplineEdge.IsBorder` renvoie la valeur `True` si l'arête de T-Spline d'entrée est une bordure.

Dans l'exemple ci-dessous, les arêtes de deux surfaces de T-Spline sont étudiées. Les surfaces sont un cylindre et sa version épaissie. Pour sélectionner toutes les arêtes, les noeuds `TSplineTopology.EdgeByIndex` sont utilisés dans les deux cas, avec l'entrée des index - une plage de nombres entiers s'étendant de 0 à n, où n est le nombre d'arêtes fourni par `TSplineTopology.EdgesCount`. Il s'agit d'une alternative à la sélection directe d'arêtes à l'aide de `TSplineTopology.DecomposedEdges`. `TSplineSurface.CompressIndices` est également utilisé dans le cas d'un cylindre épaissi pour réorganiser les index d'arête.
Un noeud `TSplineEdge.IsBorder` est utilisé pour vérifier quelles arêtes sont des arêtes de bordure. La position des arêtes de bordure du cylindre plat est mise en surbrillance à l'aide des noeuds `TSplineEdge.UVNFrame` et `TSplineUVNFrame.Position`. Le cylindre épaissi ne dispose pas d'arêtes de bordure.

## Exemple de fichier

![Example](./Autodesk.DesignScript.Geometry.TSpline.TSplineEdge.IsBorder_img.jpg)
