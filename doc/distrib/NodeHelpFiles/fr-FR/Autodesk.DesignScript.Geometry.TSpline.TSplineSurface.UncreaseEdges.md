## In-Depth
A l'opposé du noeud `TSplineSurface.CreaseEdges`, ce noeud supprime le pli de l'arête spécifiée sur une surface de T-Spline.
Dans l'exemple ci-dessous, une surface de T-Spline est générée à partir d'un tore de T-Spline. Toutes les arêtes sont sélectionnées à l'aide des noeuds `TSplineTopology.EdgeByIndex` et `TSplineTopology.EdgesCount` et le pli est appliqué à toutes les arêtes à l'aide du noeud `TSplineSurface.CreaseEdges`. Un sous-ensemble d'arêtes dont les index vont de 0 à 7 est ensuite sélectionné et l'opération inverse est appliquée, cette fois l'aide du noeud `TSplineSurface.UncreaseEdges`. La position des arêtes sélectionnées est prévisualisée à l'aide des noeuds `TSplineEdge.UVNFrame` et `TSplineUVNFrame.Position`.

## Exemple de fichier

![Example](./Autodesk.DesignScript.Geometry.TSpline.TSplineSurface.UncreaseEdges_img.jpg)
