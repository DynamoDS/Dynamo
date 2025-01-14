<!--- Autodesk.DesignScript.Geometry.TSpline.TSplineSurface.BridgeEdgesToEdges --->
<!--- NTIOVTTOXGWZ33W6ET5JH4PSYC7L3IFSWCJV4Y3IG3CTARQGOG3A --->
## Em profundidade
`TSplineSurface.BridgeEdgesToEdges` conecta dois conjuntos de arestas da mesma superfície ou de duas superfícies diferentes. O nó requer as entradas descritas abaixo. As três primeiras entradas são suficientes para gerar a ponte, sendo o resto das entradas opcional. A superfície resultante é filha da superfície à qual o primeiro grupo de arestas pertence.

- `TSplineSurface`: the surface to bridge
- `firstGroup`: Edges from the TSplineSurface selected
- `secondGroup`: arestas da mesma superfície da T-Spline selecionada ou de uma superfície diferente. O número de arestas deve coincidir em número ou ser um múltiplo do número de arestas do outro lado da ponte.
- `followCurves`: (optional) a curve for the bridge to follow. In the absence of this input, the bridge follows a straight line
- `frameRotations`: (optional) number of rotations of the bridge extrusion that connects the chosen edges.
- `spansCounts`: (optional) number of spans/segments of the bridge extrusion that connects the chosen edges. If the number of spans is too low, certain options might not be available until it is increased.
- `cleanBorderBridges`:(optional) deletes bridges between border bridges to prevent creases
- `keepSubdCreases`:(optional) preserves the SubD-creases of the input topology, resulting in a creased treatement of the start and end of the bridge
- `firstAlignVertices`(optional) and `secondAlignVertices`: enforce the alignment between two sets of vertices instead of automatically choosing to connect pairs of closest vertices.
- `flipAlignFlags`: (optional) reverses the direction of vertices to align


No exemplo abaixo, são criados dois planos da T-Spline e é excluída uma face no centro de cada um usando o nó `TSplineSurface.DeleteEdges`. As arestas em torno da face excluída são coletadas usando o nó `TSplineTopology.VertexByIndex`. Para criar uma ponte, dois grupos de arestas são usados como entrada para o `TSplineSurface.BrideEdgesToEdges`, junto com uma das superfícies. Isso cria a ponte. Mais vãos são adicionados à ponte editando a entrada `spansCounts`. Quando uma curva é usada como entrada para `followCurves`, a ponte segue a direção da curva fornecida. As entradas `keepSubdCreases`,`frameRotations`, `firstAlignVertices` e `secondAlignVertices` demonstram como a forma da ponte pode ser ajustada.

## Arquivo de exemplo

![Example](./NTIOVTTOXGWZ33W6ET5JH4PSYC7L3IFSWCJV4Y3IG3CTARQGOG3A_img.gif)

