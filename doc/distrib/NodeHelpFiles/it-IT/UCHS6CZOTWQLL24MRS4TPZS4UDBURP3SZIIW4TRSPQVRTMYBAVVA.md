<!--- Autodesk.DesignScript.Geometry.TSpline.TSplineSurface.UncreaseVertices --->
<!--- UCHS6CZOTWQLL24MRS4TPZS4UDBURP3SZIIW4TRSPQVRTMYBAVVA --->
## In-Depth
Nell'esempio seguente, viene utilizzato il nodo `TSplineSurface.UncreaseVertices` sui vertici degli angoli di una primitiva del piano. Per default, tali vertici vengono triangolari al momento della creazione della superficie. I vertici vengono identificati con l'aiuto dei nodi `TSplineVertex.UVNFrame` e `TSplineUVNFrame.Poision`, con l'opzione `Show Labels` attivata. I vertici degli angoli vengono quindi selezionati utilizzando il nodo `TSplineTopology.VertexByIndex` e ne viene annullata la triangolazione. L'effetto di questa azione può essere visualizzato in anteprima se la forma è in modalità uniforme.

## File di esempio

![Example](./UCHS6CZOTWQLL24MRS4TPZS4UDBURP3SZIIW4TRSPQVRTMYBAVVA_img.jpg)
