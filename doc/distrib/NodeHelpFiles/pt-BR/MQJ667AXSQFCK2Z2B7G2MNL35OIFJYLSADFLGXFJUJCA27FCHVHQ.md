<!--- Autodesk.DesignScript.Geometry.TSpline.TSplineSurface.BridgeFacesToFaces --->
<!--- MQJ667AXSQFCK2Z2B7G2MNL35OIFJYLSADFLGXFJUJCA27FCHVHQ --->
## Em profundidade
`TSplineSurface.BridgeEdgesToFaces` conecta dois conjuntos de faces, da mesma superfície ou de duas superfícies diferentes. O nó requer as entradas descritas abaixo. As três primeiras entradas são suficientes para gerar a ponte, sendo o resto das entradas opcional. A superfície resultante é filha da superfície à qual o primeiro grupo de arestas pertence.

No exemplo abaixo, é criada uma superfície de toroide usando `TSplineSurface.ByTorusCenterRadii`. Duas de suas faces são selecionadas e usadas como entrada para o nó `TSplineSurface.BridgeFacesToFaces`, junto com a superfície do toroide. O restante das entradas demonstra como a ponte pode ser ajustada ainda mais:
- `followCurves`: (optional) a curve for the bridge to follow. In the absence of this input, the bridge follows a straight line
- `frameRotations`: (optional) number of rotations of the bridge extrusion that connects the chosen edges.
- `spansCounts`: (optional) number of spans/segments of the bridge extrusion that connects the chosen edges. If the number of spans is too low, certain options might not be available until it is increased.
- `cleanBorderBridges`: (opcional) exclui pontes entre pontes de borda para evitar dobras.
- `keepSubdCreases`: (opcional) preserva as dobras subdivididas da topologia de entrada, resultando em um tratamento dobrado do início e do fim da ponte. A superfície do toroide não tem arestas dobradas; portanto, essa entrada não tem efeito na forma.
- `firstAlignVertices`(opcional) e `secondAlignVertices`: especificando um par deslocado de vértices, a ponte adquire uma rotação de luz.
- `flipAlignFlags`: (optional) reverses the direction of vertices to align

## Arquivo de exemplo

![Example](./MQJ667AXSQFCK2Z2B7G2MNL35OIFJYLSADFLGXFJUJCA27FCHVHQ_img.gif)
