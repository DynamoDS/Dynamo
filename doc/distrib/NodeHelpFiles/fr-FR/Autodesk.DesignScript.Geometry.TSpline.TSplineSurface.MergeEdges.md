## Description approfondie
Dans l'exemple ci-dessous, une surface de T-Spline est créée par extrusion d'une courbe NURBS. Six de ses arêtes sont sélectionnées avec un noeud `TSplineTopology.EdgeByIndex`, trois de chaque côté de la forme. Les deux jeux d'arêtes, ainsi que la surface, sont passés dans le noeud `TSplineSurface.MergeEdges`. L'ordre des groupes d'arête a une incidence sur la forme. Le premier groupe d'arêtes est déplacé pour correspondre au deuxième, qui reste au même endroit. L'entrée `insertCreases` ajoute l'option de pli de la couture le long des arêtes fusionnées. Le résultat de l'opération de fusion est converti sur le côté pour un meilleur aperçu.
___
## Exemple de fichier

![TSplineSurface.MergeEdges](./Autodesk.DesignScript.Geometry.TSpline.TSplineSurface.MergeEdges_img.gif)
