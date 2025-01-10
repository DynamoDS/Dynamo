<!--- Autodesk.DesignScript.Geometry.TSpline.TSplineSurface.BridgeFacesToEdges --->
<!--- DVNDD4ZUEDM4QCH35SLRIEZJLS266CIRRB7MZMMNDBI5W6UPBSQA --->
## Em profundidade
`TSplineSurface.BridgeToFacesToEdges` é conectado a um conjunto de arestas com um conjunto de faces, da mesma superfície ou de duas superfícies diferentes. As arestas que compõem as faces precisam coincidir em número ou ser um múltiplo das arestas do outro lado da ponte. O nó requer as entradas descritas abaixo. As três primeiras entradas são suficientes para gerar a ponte, sendo o resto das entradas opcional. A superfície resultante é filha da superfície à qual o primeiro grupo de arestas pertence.

- `TSplineSurface`: a superfície para ponte
- `firstGroup`: faces de TSplineSurface selecionadas
- `secondGroup`: arestas da mesma superfície da T-Spline selecionada ou de uma superfície diferente. O número de arestas deve coincidir ou ser um múltiplo do número de arestas do outro lado da ponte.
- `followCurves`: (opcional) uma curva a ser seguida pela ponte. Na ausência dessa entrada, a ponte segue uma linha reta
- `frameRotations`: (opcional) número de rotações da extrusão da ponte que conecta as arestas selecionadas.
- `spansCounts`: (opcional) número de vãos/segmentos da extrusão da ponte que conecta as arestas escolhidas. Se o número de vãos for muito baixo, algumas opções poderão não estar disponíveis até que seja aumentado.
- `cleanBorderBridges`: (opcional) exclui pontes entre pontes de borda para evitar dobras
- `keepSubdCreases`:(opcional) preserva as dobras subdivididas da topologia de entrada, resultando em um tratamento dobrado do início e do fim da ponte
- `firstAlignVertices`(opcional) e `secondAlignVertices`: aplique o alinhamento entre dois conjuntos de vértices em vez de escolher conectar pares de vértices mais próximos automaticamente.
- `flipAlignFlags`: (opcional) inverte a direção dos vértices para alinhar


No exemplo abaixo, são criados dois planos da T-Spline e conjuntos de arestas e faces são coletados usando os nós `TSplineTopology.VertexByIndex` e `TSplineTopology.FaceByIndex`. Para criar uma ponte, as faces e as arestas são usadas como entrada para o nó `TSplineSurface.BrideFacesToEdges`, junto com uma das superfícies. Isso cria a ponte. Mais vãos são adicionados à ponte editando a entrada `spansCounts`. Quando uma curva é usada como entrada para `followCurves`, a ponte segue a direção da curva fornecida. As entradas `keepSubdCreases`,`frameRotations`, `firstAlignVertices` e `secondAlignVertices` demonstram como as entradas da ponte podem ser ajustadas.

## Arquivo de exemplo

![BridgeFacesToEdges](./DVNDD4ZUEDM4QCH35SLRIEZJLS266CIRRB7MZMMNDBI5W6UPBSQA_img.gif)
