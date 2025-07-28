<!--- Autodesk.DesignScript.Geometry.TSpline.TSplineTopology.DecomposedFaces --->
<!--- VIA3XNZWZWW3XHWL222NGHWM22VLSA4QXMZCPWZ6JO6G3P7H2WGA --->
## En detalle:
En el ejemplo siguiente, una superficie de T-Spline plana con caras y vértices extruidos, subdivididos y estirados se inspecciona con el nodo `TSplineTopology.DecomposedFaces`, que devuelve una lista de los siguientes tipos de caras incluidos en la superficie de T-Spline:

- `all`: lista de todas las caras.
- `regular`: lista de caras normales.
- `nGons`: lista de caras de NGon.
- `border`: lista de las caras de borde.
- `inner`: lista de caras interiores.

Los nodos `TSplineFace.UVNFrame` y `TSplineUVNFrame.Position` se utilizan para resaltar los distintos tipos de caras de la superficie.
___
## Archivo de ejemplo

![TSplineTopology.DecomposedFaces](./VIA3XNZWZWW3XHWL222NGHWM22VLSA4QXMZCPWZ6JO6G3P7H2WGA_img.gif)
