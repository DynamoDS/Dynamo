<!--- Autodesk.DesignScript.Geometry.TSpline.TSplineTopology.DecomposedFaces --->
<!--- VIA3XNZWZWW3XHWL222NGHWM22VLSA4QXMZCPWZ6JO6G3P7H2WGA --->
## Description approfondie
Dans l'exemple ci-dessous, une surface T-Spline plane avec des sommets et des faces extrudés, subdivisés et étirés est inspectée avec le noeud `TSplineTopology.DecomposedFaces`, qui renvoie une liste des types de faces suivants contenus dans la surface de T-Spline :

- `all` : liste de toutes les faces
- `standard` : liste de faces normales
- `nGons` : liste des faces polygones
- `border` : liste des faces de bordure
- `inner` : liste des faces internes

Les noeuds `TSplineFace.UVNFrame` et `TSplineUVNFrame.Position` sont utilisés pour mettre en surbrillance les différents types de faces de la surface.
___
## Exemple de fichier

![TSplineTopology.DecomposedFaces](./VIA3XNZWZWW3XHWL222NGHWM22VLSA4QXMZCPWZ6JO6G3P7H2WGA_img.gif)
