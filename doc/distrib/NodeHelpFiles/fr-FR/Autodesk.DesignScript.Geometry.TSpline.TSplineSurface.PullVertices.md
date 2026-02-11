## Description approfondie
Dans l'exemple ci-dessous, tous les sommets internes d'une surface plane de T-Spline sont collectés à l'aide du noeud `TSplineTopology.InnerVertices`. Les sommets, ainsi que la surface à laquelle ils appartiennent, sont utilisés comme entrée pour le noeud `TSplineSurface.PullVertices`. L'entrée `geometry` est une sphère située au-dessus de la surface plane. L'entrée de `surfacePoints` est définie sur False et les points de contrôle sont utilisés pour effectuer l'opération d'extraction.
___
## Exemple de fichier

![TSplineSurface.PullVertices](./Autodesk.DesignScript.Geometry.TSpline.TSplineSurface.PullVertices_img.jpg)
