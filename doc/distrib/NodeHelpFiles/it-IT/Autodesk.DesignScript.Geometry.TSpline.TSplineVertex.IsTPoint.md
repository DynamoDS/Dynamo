## In-Depth
`TSplineVertex.IsTPoint` indica se un vertice è un punto a T. I punti a T sono vertici all'estremità di righe parziali di punti di controllo.

Nell'esempio seguente, `TSplineSurface.SubdivideFaces` viene utilizzato sulla primitiva di un parallelepipedo T-Spline per semplificare uno dei diversi modi di aggiungere punti a T ad una superficie. Il nodo `TSplineVertex.IsTPoint` viene utilizzato per confermare che un vertice in corrispondenza di un indice è un punto a T. Per visualizzare meglio la posizione dei punti a T, vengono utilizzati i nodi `TSplineVertex.UVNFrame` e `TSplineUVNFrame.Position`.



## File di esempio

![Example](./Autodesk.DesignScript.Geometry.TSpline.TSplineVertex.IsTPoint_img.jpg)
