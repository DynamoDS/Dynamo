## In profondità
Nell'esempio seguente, una superficie T-Spline con riflessi aggiunti viene esaminata utilizzando il nodo `TSplineSurface.Reflections`, che restituisce un elenco di tutti i riflessi applicati alla superficie. Il risultato è un elenco di due riflessi. La stessa superficie viene quindi passata attraverso un nodo `TSplineSurface.RemoveReflections` e ispezionata nuovamente. Questa volta, il nodo `TSplineSurface.Reflections` restituisce un errore, dovuto al fatto che i riflessi sono stati rimossi.
___
## File di esempio

![TSplineSurface.Reflections](./Autodesk.DesignScript.Geometry.TSpline.TSplineSurface.Reflections_img.jpg)
