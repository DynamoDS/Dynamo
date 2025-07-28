<!--- Autodesk.DesignScript.Geometry.TSpline.TSplineSurface.BridgeEdgesToEdges --->
<!--- NTIOVTTOXGWZ33W6ET5JH4PSYC7L3IFSWCJV4Y3IG3CTARQGOG3A --->
## In profondità
`TSplineSurface.BridgeEdgesToEdges` collega due gruppi di bordi della stessa superficie o di due superfici diverse. Il nodo richiede gli input descritti qui sotto. I primi tre input sono sufficienti per generare il ponte. Il resto degli input è facoltativo. La superficie risultante è un elemento derivato della superficie a cui appartiene il primo gruppo di bordi.

- `TSplineSurface`: the surface to bridge
- `firstGroup`: Edges from the TSplineSurface selected
- `secondGroup`: bordi della stessa superficie T-Spline selezionata o di un'altra superficie. Il numero di bordi deve corrispondere o essere un multiplo del numero di bordi sull'altro lato del ponte.
- `followCurves`: (optional) a curve for the bridge to follow. In the absence of this input, the bridge follows a straight line
- `frameRotations`: (optional) number of rotations of the bridge extrusion that connects the chosen edges.
- `spansCounts`: (optional) number of spans/segments of the bridge extrusion that connects the chosen edges. If the number of spans is too low, certain options might not be available until it is increased.
- `cleanBorderBridges`:(optional) deletes bridges between border bridges to prevent creases
- `keepSubdCreases`:(optional) preserves the SubD-creases of the input topology, resulting in a creased treatement of the start and end of the bridge
- `firstAlignVertices`(optional) and `secondAlignVertices`: enforce the alignment between two sets of vertices instead of automatically choosing to connect pairs of closest vertices.
- `flipAlignFlags`: (optional) reverses the direction of vertices to align


Nell'esempio seguente, vengono creati due piani T-Spline e viene eliminata una faccia al centro di ciascuno utilizzando il nodo `TSplineSurface.DeleteEdges`. I bordi attorno alla faccia eliminata vengono raccolti utilizzando il nodo `TSplineTopology.VertexByIndex`. Per creare un ponte, vengono utilizzati due gruppi di bordi come input per `TSplineSurface.BrideEdgesToEdges`, insieme ad una delle superfici. In questo modo viene creato il ponte. Vengono aggiunte più campate al ponte modificando l'input `spansCounts`. Quando una curva viene utilizzata come input per `followCurves`, il ponte segue la direzione della curva fornita. Gli input `keepSubdCreases`, `frameRotations`, `firstAlignVertices` e `secondAlignVertices` mostrano come la forma del ponte può essere perfezionata.

## File di esempio

![Example](./NTIOVTTOXGWZ33W6ET5JH4PSYC7L3IFSWCJV4Y3IG3CTARQGOG3A_img.gif)

