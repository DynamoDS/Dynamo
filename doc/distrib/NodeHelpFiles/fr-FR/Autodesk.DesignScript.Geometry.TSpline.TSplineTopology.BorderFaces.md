## Description approfondie
`TSplineTopology.BorderFaces` renvoie la liste des faces de bordure contenues dans la surface de T-Spline.

Dans l'exemple ci-dessous, deux surfaces de T-Spline sont créées par l'intermédiaire de `TSplineSurface.ByCylinderPointsRadius`. L'une est une surface ouverte, tandis que l'autre est épaissie à l'aide de `TSplineSurface.Thicken`, ce qui la transforme en surface fermée. Lorsque les deux sont examinées avec le noeud `TSplineTopology.BorderFaces`, la première renvoie une liste de faces de bordure tandis que la seconde renvoie une liste vide. En effet, étant donné que la surface est fermée, elle ne comporte pas d'arêtes de bordure.
___
## Exemple de fichier

![TSplineTopology.BorderFaces](./Autodesk.DesignScript.Geometry.TSpline.TSplineTopology.BorderFaces_img.jpg)
