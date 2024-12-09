<!--- Autodesk.DesignScript.Geometry.TSpline.TSplineTopology.DecomposedVertices --->
<!--- GNCPW2XCPOITIUUDNZKISNP347TEGTDWVI5SQTBJX3JWFZTVRXGA --->
## Description approfondie
Dans l'exemple ci-dessous, une surface de plane de T-Spline aux sommets et aux faces extrudés, subdivisés et étirés est inspectée avec le noeud `TSplineTopology.DecomposedVertices`, qui renvoie une liste des types de sommets suivants contenus dans la surface de T-Spline :

- `tous` : liste de tous les sommets
- `standard` : liste de sommets réguliers
- `tPoints` : liste des sommets de T-Point
- `starPoints` : liste des sommets du Point d'étoile
- `nonManifold` : liste des sommets Non-Manifold
- `border` : liste des sommets de bordures
- `inner` : liste des sommets internes

Les noeuds `TSplineVertex.UVNFrame` et `TSplineUVNFrame.Position` sont utilisés pour mettre en surbillance les différents types de sommets de la surface.

___
## Exemple de fichier

![TSplineTopology.DecomposedVertices](./GNCPW2XCPOITIUUDNZKISNP347TEGTDWVI5SQTBJX3JWFZTVRXGA_img.gif)
