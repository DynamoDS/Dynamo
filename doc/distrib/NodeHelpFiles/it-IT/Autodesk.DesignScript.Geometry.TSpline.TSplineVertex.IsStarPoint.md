## In-Depth
`TSplineVertex.IsStarPoint` indica se un vertice è un punto a stella.

I punti a stella esistono quando si uniscono 3, 5 o più bordi. Si trovano naturalmente nella primitiva del parallelepipedo o quadball e vengono generalmente creati quando si estrude una faccia T-Spline, si elimina una faccia o si esegue l'unione. Diversamente dai vertici regolari e con punti a T, i punti a stella non sono controllati da righe rettangolari di punti di controllo. I punti a stella rendono l'area attorno a essi più difficile da controllare e possono creare distorsione, pertanto dovrebbero essere utilizzati solo laddove necessario. Tra le posizioni non adatte per il posizionamento dei punti a stella vi sono le parti più nitide del modello, come i bordi triangolati, le parti in cui la curvatura cambia in modo significativo o il bordo di una superficie aperta.

I punti a stella determinano inoltre il modo in cui una T-Spline verrà convertita in rappresentazione del contorno (BREP). Quando una T-Spline viene convertita in BREP, verrà divisa in superfici separate in corrispondenza di ciascun punto a stella.

Nell'esempio seguente, `TSplineVertex.IsStarPoint` viene utilizzato per eseguire query se il vertice selezionato con `TSplineTopology.VertexByIndex` è un punto a stella.


## File di esempio

![Example](./Autodesk.DesignScript.Geometry.TSpline.TSplineVertex.IsStarPoint_img.jpg)
