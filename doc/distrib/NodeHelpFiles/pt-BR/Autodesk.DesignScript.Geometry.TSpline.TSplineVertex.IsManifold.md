## In-Depth
No exemplo abaixo, uma superfície não múltipla é produzida unindo duas superfícies que compartilham uma aresta interna. Isso resulta em uma superfície que não tem parte frontal e posterior claras. A superfície não múltipla somente pode ser exibida no modo de caixa até que seja corrigida. `TSplineTopology.DecomposedVertices` é usado para consultar todos os vértices da superfície e o nó `TSplineVertex.IsManifold`é usado para realçar quais dos vértices se qualificam como múltiplos. Os vértices não múltiplos são extraídos e sua posição é visualizada usando os nós `TSplineVertex.UVNFrame` e `TSplineUVNFrame.Position`.


## Arquivo de exemplo

![Example](./Autodesk.DesignScript.Geometry.TSpline.TSplineVertex.IsManifold_img.jpg)
