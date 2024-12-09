<!--- Autodesk.DesignScript.Geometry.TSpline.TSplineTopology.DecomposedEdges --->
<!--- 7LMFKLQNCV53W7KLS5QWD3E27NGGA33QPHSXMUGH323WVXWJY3GQ --->
## Em profundidade
No exemplo abaixo, uma superfície da T-Spline plana com vértices e faces extrudados, subdivididos e extraídos é inspecionada com o nó `TSplineTopology.DecomposedEdges`, que retorna uma lista dos seguintes tipos de arestas contidas na superfície da T-Spline:

- `all`: lista de todas as arestas
- `nonManifold`: lista de arestas não múltiplas
- `border`: lista de arestas de borda
- `inner`: lista de arestas internas


O nó `Edge.CurveGeometry` é usado para realçar os diferentes tipos de arestas da superfície.
___
## Arquivo de exemplo

![TSplineTopology.DecomposedEdges](./7LMFKLQNCV53W7KLS5QWD3E27NGGA33QPHSXMUGH323WVXWJY3GQ_img.gif)
