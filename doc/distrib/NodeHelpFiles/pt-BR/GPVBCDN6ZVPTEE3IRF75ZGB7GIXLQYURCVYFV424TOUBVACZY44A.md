<!--- Autodesk.DesignScript.Geometry.TSpline.TSplineSurface.BridgeEdgesToFaces --->
<!--- GPVBCDN6ZVPTEE3IRF75ZGB7GIXLQYURCVYFV424TOUBVACZY44A --->
## Em profundidade
`TSplineSurface.BridgeEdgesToFaces` conecta um conjunto de arestas com um conjunto de faces, da mesma superfície ou de duas superfícies diferentes. As arestas que compõem as faces precisam coincidir em número ou ser um múltiplo das arestas do outro lado da ponte. O nó requer as entradas descritas abaixo. As três primeiras entradas são suficientes para gerar a ponte, sendo o resto das entradas opcional. A superfície resultante é filha da superfície à qual o primeiro grupo de arestas pertence.

- `TSplineSurface`: the surface to bridge
- `firstGroup`: arestas da TSplineSurface selecionada
- `secondGroup`: faces da mesma superfície da T-Spline selecionada ou de uma superfície diferente.
- `followCurves`: (optional) a curve for the bridge to follow. In the absence of this input, the bridge follows a straight line
- `frameRotations`: (optional) number of rotations of the bridge extrusion that connects the chosen edges.
- `spansCounts`: (optional) number of spans/segments of the bridge extrusion that connects the chosen edges. If the number of spans is too low, certain options might not be available until it is increased.
- `cleanBorderBridges`:(optional) deletes bridges between border bridges to prevent creases
- `keepSubdCreases`:(optional) preserves the SubD-creases of the input topology, resulting in a creased treatement of the start and end of the bridge
- `firstAlignVertices`(optional) and `secondAlignVertices`: enforce the alignment between two sets of vertices instead of automatically choosing to connect pairs of closest vertices.
- `flipAlignFlags`: (optional) reverses the direction of vertices to align


No exemplo abaixo, são criados dois planos da T-Spline e os conjuntos de arestas e faces são coletados usando os nós `TSplineTopology.VertexByIndex` e `TSplineTopology.FaceByIndex`. Para criar uma ponte, as faces e as arestas são usadas como entrada para o nó `TSplineSurface.BrideEdgesToFaces`, junto com uma das superfícies. Isso cria a ponte. Mais vãos são adicionados à ponte editando a entrada `spansCounts`. Quando uma curva é usada como entrada para `followCurves`, a ponte segue a direção da curva fornecida. As entradas `keepSubdCreases`,`frameRotations`, `firstAlignVertices` e `secondAlignVertices` demonstram como as entradas da ponte podem ser ajustadas.

## Arquivo de exemplo

![Example](./GPVBCDN6ZVPTEE3IRF75ZGB7GIXLQYURCVYFV424TOUBVACZY44A_img.gif)

