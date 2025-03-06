<!--- Autodesk.DesignScript.Geometry.TSpline.TSplineSurface.BridgeFacesToEdges --->
<!--- DVNDD4ZUEDM4QCH35SLRIEZJLS266CIRRB7MZMMNDBI5W6UPBSQA --->
## In profondità
`TSplineSurface.BridgeToFacesToEdges` collega un gruppo di bordi con un gruppo di facce, dalla stessa superficie o da due superfici diverse. I bordi che compongono le facce devono corrispondere come numero o essere un multiplo dei bordi sull'altro lato del ponte. Il nodo richiede gli input descritti qui sotto. I primi tre input sono sufficienti per generare il ponte. Il resto degli input è facoltativo. La superficie risultante è un elemento derivato della superficie a cui appartiene il primo gruppo di bordi.

- `TSplineSurface`: la superficie da unire
- `firstGroup`: facce dell'oggetto TSplineSurface selezionato
- `secondGroup`: bordi della stessa superficie T-Spline selezionata o di un'altra superficie. Il numero di bordi deve corrispondere o essere un multiplo del numero di bordi sull'altro lato del ponte.
- `followCurves`: una curva (facoltativa) che il ponte deve seguire. In assenza di questo input, il ponte segue una linea retta
- `frameRotations`: numero (facoltativo) di rotazioni dell'estrusione del ponte che collega i bordi scelti.
- `spansCounts`: numero (facoltativo) di campate/segmenti dell'estrusione del ponte che collegano i bordi scelti. Se il numero di segmenti è troppo basso, alcune opzioni potrebbero non essere disponibili fino a quando non viene aumentato.
- `cleanBorderBridges`: (facoltativo) elimina i ponti tra i ponti dei bordi per evitare le triangolazioni
- `keepSubdCreases`: (facoltativo) mantiene le triangolazioni secondarie della topologia di input, con conseguente trattamento triangolato dell'inizio e della fine del ponte
- `firstAlignVertices` (opzionale) e `secondAlignVertices`: applicano l'allineamento tra due gruppi di vertici anziché scegliere automaticamente di collegare le coppie dei vertici più vicini.
- `flipAlignFlags`: (facoltativo) inverte la direzione dei vertici da allineare


Nell'esempio seguente, vengono creati due piani T-Spline e vengono raccolti gruppi di bordi e facce utilizzando `TSplineTopology.VertexByIndex` e `TSplineTopology.FaceByIndex`. Per creare un ponte, le facce e i bordi vengono utilizzati come input per il nodo `TSplineSurface.BrideFacesToEdges`, insieme ad una delle superfici. In questo modo viene creato il ponte. Vengono aggiunte più campate al ponte modificando l'input `spansCounts`. Quando una curva viene utilizzata come input per `followCurves`, il ponte segue la direzione della curva fornita. Gli input `keepSubdCreases`, `frameRotations`, `firstAlignVertices` e `secondAlignVertices` mostrano come la forma del ponte può essere perfezionata.

## File di esempio

![BridgeFacesToEdges](./DVNDD4ZUEDM4QCH35SLRIEZJLS266CIRRB7MZMMNDBI5W6UPBSQA_img.gif)
