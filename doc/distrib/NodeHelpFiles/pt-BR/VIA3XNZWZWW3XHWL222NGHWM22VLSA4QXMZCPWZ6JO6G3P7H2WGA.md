<!--- Autodesk.DesignScript.Geometry.TSpline.TSplineTopology.DecomposedFaces --->
<!--- VIA3XNZWZWW3XHWL222NGHWM22VLSA4QXMZCPWZ6JO6G3P7H2WGA --->
## Em profundidade
No exemplo abaixo, uma superfície da T-Spline plana com vértices e faces extrudados, subdivididos e extraídos é inspecionada com o nó `TSplineTopology.DecomposedFaces`, que retorna uma lista dos seguintes tipos de faces contidas na superfície da TSpline:

- `all`: lista de todas as faces
- `regular`: lista de faces regulares
- `nGons`: lista de faces NGon
- `border`: lista de faces de borda
- `inner`: lista de faces internas

Os nós `TSplineFace.UVNFrame` e `TSplineUVNFrame.Position` são usados para realçar os diferentes tipos de faces da superfície.
___
## Arquivo de exemplo

![TSplineTopology.DecomposedFaces](./VIA3XNZWZWW3XHWL222NGHWM22VLSA4QXMZCPWZ6JO6G3P7H2WGA_img.gif)
