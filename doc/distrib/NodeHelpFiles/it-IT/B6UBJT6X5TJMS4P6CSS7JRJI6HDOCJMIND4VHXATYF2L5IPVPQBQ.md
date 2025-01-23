<!--- Autodesk.DesignScript.Geometry.TSpline.TSplineSurface.RemoveReflections --->
<!--- B6UBJT6X5TJMS4P6CSS7JRJI6HDOCJMIND4VHXATYF2L5IPVPQBQ --->
## In-Depth
`TSplineSurface.RemoveReflections` rimuove i riflessi dall'input `tSplineSurface`. La rimozione dei riflessi non modifica la forma, ma interrompe la dipendenza tra le parti riflesse della geometria, consentendo di modificarle in modo indipendente.

Nell'esempio seguente, viene creata per prima cosa una superficie T-Spline applicando riflessi assiali e radiali. La superficie viene quindi passata al nodo `TSplineSurface.RemoveReflections`, rimuovendo i riflessi. Per illustrare in che modo ciò influisce sulle successive alterazioni, uno dei vertici viene spostato utilizzando un nodo `TSplineSurface.MoveVertex`. Poiché i riflessi vengono rimossi dalla superficie, viene modificato solo uno vertice.

## File di esempio

![Example](./B6UBJT6X5TJMS4P6CSS7JRJI6HDOCJMIND4VHXATYF2L5IPVPQBQ_img.jpg)
