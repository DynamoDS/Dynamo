<!--- Autodesk.DesignScript.Geometry.TSpline.TSplineTopology.DecomposedVertices --->
<!--- GNCPW2XCPOITIUUDNZKISNP347TEGTDWVI5SQTBJX3JWFZTVRXGA --->
## Em profundidade
No exemplo abaixo, uma superfície da T-Spline plana com vértices e faces extrudados, subdivididos e extraídos é inspecionada com o nó `TSplineTopology.DecomposedVertices`, que retorna uma lista dos seguintes tipos de vértices contidos na superfície da T-Spline:

- `all`: lista de todos os vértices
- `regular`: lista de faces regulares
- `tPoints`: lista de vértices de ponto T
- `starPoints`: lista de vértices de ponto de estrela
- `nonManifold`: lista de vértices não múltiplos
- `border`: lista de vértices de borda
- `inner`: lista de vértices internos

Os nós `TSplineVertex.UVNFrame` e `TSplineUVNFrame.Position` são usados para realçar os diferentes tipos de vértices da superfície.

___
## Arquivo de exemplo

![TSplineTopology.DecomposedVertices](./GNCPW2XCPOITIUUDNZKISNP347TEGTDWVI5SQTBJX3JWFZTVRXGA_img.gif)
