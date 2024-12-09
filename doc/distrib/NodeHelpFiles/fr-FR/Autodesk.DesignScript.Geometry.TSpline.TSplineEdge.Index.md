## In-Depth
Notez que dans une topologie de surface de T-Spline, les index `Face`, `Edge` et `Vertex` ne coïncident pas nécessairement avec le numéro de séquence de l'élément dans la liste. Utilisez le noeud `TSplineSurface.CompressIndices` pour résoudre ce problème.

Dans l'exemple ci-dessous, `TSplineTopology.DecomposedEdges` est utilisé pour extraire les arêtes de bordure d'une surface de T-Spline. Un noeud `TSplineEdge.Index` est ensuite utilisé pour obtenir les index des arêtes fournies.

## Exemple de fichier

![Example](./Autodesk.DesignScript.Geometry.TSpline.TSplineEdge.Index_img.jpg)
