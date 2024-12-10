## Description approfondie
`TSplineTopology.BorderEdges` renvoie une liste d'arêtes de bordure contenues dans la surface de T-Spline.

Dans l'exemple ci-dessous, deux surfaces de T-Spline sont créées par l'intermédiaire de `TSplineSurface.ByCylinderPointsRadius`. L'une est une surface ouverte et l'autre est épaissie à l'aide de `TSplineSurface.Thicken`, qui la transforme en surface fermée. Lorsque les deux sont examinées avec le noeud `TSplineTopology.BorderEdges`, la première renvoie une liste d'arêtes de bordures tandis que la seconde renvoie une liste vide. En effet, étant donné que la surface est fermée, elle ne comporte pas d'arêtes de bordure.
___
## Exemple de fichier

![TSplineTopology.BorderEdges](./Autodesk.DesignScript.Geometry.TSpline.TSplineTopology.BorderEdges_img.jpg)
