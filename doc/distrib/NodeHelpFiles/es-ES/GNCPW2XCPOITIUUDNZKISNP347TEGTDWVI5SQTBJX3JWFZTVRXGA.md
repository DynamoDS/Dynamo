<!--- Autodesk.DesignScript.Geometry.TSpline.TSplineTopology.DecomposedVertices --->
<!--- GNCPW2XCPOITIUUDNZKISNP347TEGTDWVI5SQTBJX3JWFZTVRXGA --->
## En detalle:
En el ejemplo siguiente, una superficie de T-Spline plana con caras y vértices extruidos, subdivididos y estirados se inspecciona con el nodo `TSplineTopology.DecomposedVertices`, que devuelve una lista de los siguientes tipos de vértices incluidos en la superficie de T-Spline:

- `all`: lista de todos los vértices.
- `regular`: lista de vértices normales.
- `tPoints`: lista de vértices de puntos T.
- `starPoints`: lista de vértices de puntos de estrella.
- `nonManifold`: lista de vértices no múltiples.
- `border`: lista de vértices de borde.
- `inner`: lista de vértices interiores.

Los nodos `TSplineVertex.UVNFrame` y `TSplineUVNFrame.Position` se utilizan para resaltar los distintos tipos de vértices de la superficie.

___
## Archivo de ejemplo

![TSplineTopology.DecomposedVertices](./GNCPW2XCPOITIUUDNZKISNP347TEGTDWVI5SQTBJX3JWFZTVRXGA_img.gif)
