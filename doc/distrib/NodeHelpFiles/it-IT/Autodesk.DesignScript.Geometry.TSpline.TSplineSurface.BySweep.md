## In profondità
Nell'esempio seguente, una superficie T-Spline viene creata eseguendo l'estrusione su percorso di un `profile` attorno ad un determinato valore`path`. L'input `parallel` controlla se le campate del profilo restano parallele alla direzione del percorso o ruotano lungo di essa. La definizione della forma è impostata da `pathSpans` e `radialSpans`. L'input `pathUniform` definisce se le campate del percorso sono distribuite in modo uniforme o tenendo conto della curvatura. Un'impostazione simile, `profileUniform`, controlla le campate lungo il profilo. La simmetria iniziale della forma è specificata dall'input `symmetry`. Infine, l'input `inSmoothMode` viene utilizzato per passare dall'anteprima in modalità uniforme a quella in modalità riquadro e viceversa della superficie T-Spline.

## File di esempio

![Example](./Autodesk.DesignScript.Geometry.TSpline.TSplineSurface.BySweep_img.jpg)
