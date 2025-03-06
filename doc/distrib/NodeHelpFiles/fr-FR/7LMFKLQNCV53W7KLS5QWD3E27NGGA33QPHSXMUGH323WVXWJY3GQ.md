<!--- Autodesk.DesignScript.Geometry.TSpline.TSplineTopology.DecomposedEdges --->
<!--- 7LMFKLQNCV53W7KLS5QWD3E27NGGA33QPHSXMUGH323WVXWJY3GQ --->
## Description approfondie
Dans l'exemple ci-dessous, une surface de T-Spline plane avec des sommets et des faces extrudés, subdivisés et tirés est inspectée à l'aide du noeud `TSplineTopology.DecomposedEdges`, qui renvoie une liste des types d'arêtes suivants contenus dans la surface de T-Spline :

- `all` : liste de tous les bords
- `nonManifold` : liste des bords non-manifold
- `border` : liste des bords de bordure
- `inner` : liste des bords intérieurs


Le noeud `Edge.CurveGeometry` est utilisé pour mettre en surbrillance les différents types d'arêtes de la surface.
___
## Exemple de fichier

![TSplineTopology.DecomposedEdges](./7LMFKLQNCV53W7KLS5QWD3E27NGGA33QPHSXMUGH323WVXWJY3GQ_img.gif)
