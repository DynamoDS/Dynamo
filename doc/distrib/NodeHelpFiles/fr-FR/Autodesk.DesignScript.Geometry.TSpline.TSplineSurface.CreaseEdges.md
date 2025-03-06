## In-Depth
`TSplineSurface.CreaseEdges` ajoute un pli prononcé à l'arête spécifiée sur une surface de T-Spline.
Dans l'exemple ci-dessous, une surface de T-Spline est générée à partir d'un tore de T-Spline. Une arête est sélectionnée à l'aide du noeud `TSplineTopology.EdgeByIndex` et un pli est appliqué à cette arête à l'aide du noeud `TSplineSurface.CreaseEdges`. Les sommets des deux arêtes de l'arête sont également pliés. La position de l'arête sélectionnée peut être prévisualisée à l'aide des noeuds `TSplineEdge.UVNFrame` et `TSplineUVNFrame.Position`.

## Exemple de fichier

![Example](./Autodesk.DesignScript.Geometry.TSpline.TSplineSurface.CreaseEdges_img.jpg)
