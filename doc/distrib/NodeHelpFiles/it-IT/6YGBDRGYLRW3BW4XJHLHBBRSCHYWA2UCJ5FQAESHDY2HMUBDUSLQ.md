<!--- Autodesk.DesignScript.Geometry.TSpline.TSplineSurface.AddReflections --->
<!--- 6YGBDRGYLRW3BW4XJHLHBBRSCHYWA2UCJ5FQAESHDY2HMUBDUSLQ --->
## In-Depth
`TSplineSurface.AddReflections` crea una nuova superficie T-Spline applicando uno o più riflessi all'input `tSplineSurface`. L'input booleano `weldSymmetricPortions` determina se i bordi triangolati generati dal riflesso vengono lisciati o mantenuti.

L'esempio seguente illustra come aggiungere più riflessi ad una superficie T-Spline utilizzando il nodo `TSplineSurface.AddReflections`. Vengono creati due riflessi: uno assiale e uno radiale. La geometria di base è una superficie T-Spline a forma di estrusione su percorso con la traiettoria di un arco. I due riflessi vengono uniti in un elenco e utilizzati come input per il nodo `TSplineSurface.AddReflections`, insieme alla geometria di base da riflettere. Gli oggetti TSplineSurface vengono saldati, producendo un oggetto TSplineSurface lisciato senza bordi triangolati. La superficie viene ulteriormente alterata spostando un vertice utilizzando il nodo `TSplineSurface.MoveVertex`. A causa del riflesso applicato alla superficie T-Spline, il movimento del vertice viene riprodotto 16 volte.

## File di esempio

![Example](./6YGBDRGYLRW3BW4XJHLHBBRSCHYWA2UCJ5FQAESHDY2HMUBDUSLQ_img.jpg)
