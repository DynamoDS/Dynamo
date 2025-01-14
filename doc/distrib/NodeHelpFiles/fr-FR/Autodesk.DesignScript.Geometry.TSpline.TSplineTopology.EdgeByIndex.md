## Description approfondie
Dans l'exemple ci-dessous, une boîte T-Spline est créée à l'aide du noeud `TSplineSurface.ByBoxLengths` avec une origine, une largeur, une longueur, une hauteur, des segments et une symétrie spécifiés.
L'option `EdgeByIndex` est ensuite utilisée pour sélectionner une arête dans la liste des arêtes de la surface générée. L'arête sélectionnée est ensuite déplacée le long des arêtes voisines à l'aide de `TSplineSurface.SlideEdges`, suivi de ses équivalents symétriques.
___
## Exemple de fichier

![TSplineTopology.EdgeByIndex](./Autodesk.DesignScript.Geometry.TSpline.TSplineTopology.EdgeByIndex_img.jpg)
