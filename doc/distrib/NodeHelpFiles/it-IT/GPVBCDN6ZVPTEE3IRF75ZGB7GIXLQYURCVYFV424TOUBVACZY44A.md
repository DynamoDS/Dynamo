<!--- Autodesk.DesignScript.Geometry.TSpline.TSplineSurface.BridgeEdgesToFaces --->
<!--- GPVBCDN6ZVPTEE3IRF75ZGB7GIXLQYURCVYFV424TOUBVACZY44A --->
## In profondità
`TSplineSurface.BridgeEdgesToFaces` collega un gruppo di bordi con un gruppo di facce, dalla stessa superficie o da due superfici diverse. I bordi che compongono le facce devono corrispondere come numero o essere un multiplo dei bordi sull'altro lato del ponte. Il nodo richiede gli input descritti qui sotto. I primi tre input sono sufficienti per generare il ponte. Il resto degli input è facoltativo. La superficie risultante è un elemento derivato della superficie a cui appartiene il primo gruppo di bordi.

- `TSplineSurface`: the surface to bridge
- `firstGroup`: bordi dell'oggetto TSplineSurface selezionato
- `secondGroup`: facce della stessa superficie T-Spline selezionata o di un'altra superficie.
- `followCurves`: (optional) a curve for the bridge to follow. In the absence of this input, the bridge follows a straight line
- `frameRotations`: (optional) number of rotations of the bridge extrusion that connects the chosen edges.
- `spansCounts`: (optional) number of spans/segments of the bridge extrusion that connects the chosen edges. If the number of spans is too low, certain options might not be available until it is increased.
- `cleanBorderBridges`:(optional) deletes bridges between border bridges to prevent creases
- `keepSubdCreases`:(optional) preserves the SubD-creases of the input topology, resulting in a creased treatement of the start and end of the bridge
- `firstAlignVertices`(optional) and `secondAlignVertices`: enforce the alignment between two sets of vertices instead of automatically choosing to connect pairs of closest vertices.
- `flipAlignFlags`: (optional) reverses the direction of vertices to align


Nell'esempio seguente, vengono creati due piani T-Spline e vengono raccolti gruppi di bordi e facce utilizzando i nodi `TSplineTopology.VertexByIndex` e `TSplineTopology.FaceByIndex`. Per creare un ponte, le facce e i bordi vengono utilizzati come input per il nodo `TSplineSurface.BrideEdgesToFaces`, insieme ad una delle superfici. In questo modo viene creato il ponte. Vengono aggiunte più campate al ponte modificando l'input `spansCounts`. Quando una curva viene utilizzata come input per `followCurves`, il ponte segue la direzione della curva fornita. Gli input `keepSubdCreases`, `frameRotations`, `firstAlignVertices` e `secondAlignVertices` mostrano come la forma del ponte può essere perfezionata.

## File di esempio

![Example](./GPVBCDN6ZVPTEE3IRF75ZGB7GIXLQYURCVYFV424TOUBVACZY44A_img.gif)

