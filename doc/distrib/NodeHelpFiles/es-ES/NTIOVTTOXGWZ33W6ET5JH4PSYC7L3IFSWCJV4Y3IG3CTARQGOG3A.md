<!--- Autodesk.DesignScript.Geometry.TSpline.TSplineSurface.BridgeEdgesToEdges --->
<!--- NTIOVTTOXGWZ33W6ET5JH4PSYC7L3IFSWCJV4Y3IG3CTARQGOG3A --->
## En detalle:
`TSplineSurface.BridgeEdgesToEdges` conecta dos conjuntos de aristas de la misma superficie o de dos diferentes. El nodo requiere las entradas que se describen a continuación. Las tres primeras entradas son suficientes para generar el puente; el resto son opcionales. La superficie resultante procede de la superficie a la que pertenece el primer grupo de aristas.

- `TSplineSurface`: the surface to bridge
- `firstGroup`: Edges from the TSplineSurface selected
- `secondGroup`: las aristas de la misma superficie de T-Spline seleccionada o de otra distinta. El número de aristas debe coincidir exactamente o ser múltiplo del número de aristas del otro lado del puente.
- `followCurves`: (optional) a curve for the bridge to follow. In the absence of this input, the bridge follows a straight line
- `frameRotations`: (optional) number of rotations of the bridge extrusion that connects the chosen edges.
- `spansCounts`: (optional) number of spans/segments of the bridge extrusion that connects the chosen edges. If the number of spans is too low, certain options might not be available until it is increased.
- `cleanBorderBridges`:(optional) deletes bridges between border bridges to prevent creases
- `keepSubdCreases`:(optional) preserves the SubD-creases of the input topology, resulting in a creased treatement of the start and end of the bridge
- `firstAlignVertices`(optional) and `secondAlignVertices`: enforce the alignment between two sets of vertices instead of automatically choosing to connect pairs of closest vertices.
- `flipAlignFlags`: (optional) reverses the direction of vertices to align


En el ejemplo siguiente, se crean dos planos de T-Spline y se suprime una cara en el centro de cada uno mediante el nodo `TSplineSurface.DeleteEdges`. Las aristas alrededor de la cara suprimida se recopilan mediante el nodo `TSplineTopology.VertexByIndex`. Para crear un puente, se utilizan dos grupos de aristas como entrada para el nodo `TSplineSurface.BrideEdgesToEdges`, junto con una de las superficies. De este modo, se crea el puente. Se añaden más tramos al puente mediante la edición de la entrada `spansCounts`. Cuando se utiliza una curva como entrada para `followCurves`, el puente sigue la dirección de la curva indicada. Las entradas `keepSubdCreases`, `frameRotations`, `firstAlignVertices` y `secondAlignVertices` muestran cómo puede ajustarse la forma del puente.

## Archivo de ejemplo

![Example](./NTIOVTTOXGWZ33W6ET5JH4PSYC7L3IFSWCJV4Y3IG3CTARQGOG3A_img.gif)

