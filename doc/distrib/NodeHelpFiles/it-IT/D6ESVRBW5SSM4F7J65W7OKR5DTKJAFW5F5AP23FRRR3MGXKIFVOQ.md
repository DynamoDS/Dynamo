<!--- Autodesk.DesignScript.Geometry.TSpline.TSplineSurface.UnweldVertices --->
<!--- D6ESVRBW5SSM4F7J65W7OKR5DTKJAFW5F5AP23FRRR3MGXKIFVOQ --->
## In-Depth
Analogamente a `TSplineSurface.UnweldEdges`, questo nodo esegue l'operazione di annullamento della saldatura su un gruppo di vertici. Di conseguenza, verrà annullata la saldatura di tutti i bordi che si uniscono in corrispondenza del vertice selezionato. A differenza dell'operazione di annullamento della triangolazione, che crea una transizione netta attorno al vertice mantenendo la connessione, l'annullamento della saldatura crea una discontinuità.

Nell'esempio seguente, verrà annullata la saldatura di uno dei vertici selezionati di un piano T-Spline con il nodo `TSplineSurface.UnweldVertices`. Viene introdotta una discontinuità lungo i bordi che circondano il vertice scelto, che è illustrata mediante l'estrazione di un vertice verso l'alto con il nodo `TSplineSurface.MoveVertices`.

## File di esempio

![Example](./D6ESVRBW5SSM4F7J65W7OKR5DTKJAFW5F5AP23FRRR3MGXKIFVOQ_img.jpg)
