<!--- Autodesk.DesignScript.Geometry.TSpline.TSplineSurface.WeldCoincidentVertices --->
<!--- UZA22A4OYIXSIP3U5CUGNZ3WBDHIEMOS2MYI5GKTJJJFBTGI5JTA --->
## In profondità
Nell'esempio seguente, due metà di una superficie T-Spline vengono unite in una mediante il nodo `TSplineSurface.ByCombinedTSplineSurfaces`. I vertici lungo il piano speculare si sovrappongono, il che diventa visibile quando uno dei vertici viene spostato utilizzando il nodo `TSplineSurface.MoveVertices`. Per risolvere questo problema, la saldatura viene eseguita utilizzando il nodo `TSplineSurface.WeldCoincidentVertices`. Il risultato dello spostamento di un vertice è ora diverso, traslato sul lato per una migliore anteprima.
___
## File di esempio

![TSplineSurface.WeldCoincidentVertices](./UZA22A4OYIXSIP3U5CUGNZ3WBDHIEMOS2MYI5GKTJJJFBTGI5JTA_img.jpg)
