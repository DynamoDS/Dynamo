<!--- Autodesk.DesignScript.Geometry.TSpline.TSplineSurface.BridgeFacesToFaces --->
<!--- MQJ667AXSQFCK2Z2B7G2MNL35OIFJYLSADFLGXFJUJCA27FCHVHQ --->
## In profondità
`TSplineSurface.BridgeEdgesToFaces` collega due gruppi di facce, della stessa superficie o di due superfici diverse. Il nodo richiede gli input descritti qui sotto. I primi tre input sono sufficienti per generare il ponte. Il resto degli input è facoltativo. La superficie risultante è un elemento derivato della superficie a cui appartiene il primo gruppo di bordi.

Nell'esempio seguente, la superficie di un toro viene creata utilizzando `TSplineSurface.ByTorusCenterRadii`. Due delle sue facce sono selezionate e utilizzate come input per il nodo `TSplineSurface.BridgeFacesToFaces`, insieme alla superficie del toro. Gli altri input mostrano come è possibile modificare ulteriormente il ponte:
- `followCurves`: (optional) a curve for the bridge to follow. In the absence of this input, the bridge follows a straight line
- `frameRotations`: (optional) number of rotations of the bridge extrusion that connects the chosen edges.
- `spansCounts`: (optional) number of spans/segments of the bridge extrusion that connects the chosen edges. If the number of spans is too low, certain options might not be available until it is increased.
- `cleanBorderBridges`: (facoltativo) elimina i ponti tra i ponti dei bordi per evitare le triangolazioni.
- `keepSubdCreases`: (facoltativo) mantiene le triangolazioni secondarie della topologia di input, con conseguente trattamento triangolato dell'inizio e della fine del ponte. La superficie del toro non presenta bordi triangolati, pertanto questo input non ha alcun effetto sulla forma.
- `firstAlignVertices` (facoltativo) e `secondAlignVertices`: specificando una coppia spostata di vertici, il ponte acquisisce una leggera rotazione.
- `flipAlignFlags`: (optional) reverses the direction of vertices to align

## File di esempio

![Example](./MQJ667AXSQFCK2Z2B7G2MNL35OIFJYLSADFLGXFJUJCA27FCHVHQ_img.gif)
