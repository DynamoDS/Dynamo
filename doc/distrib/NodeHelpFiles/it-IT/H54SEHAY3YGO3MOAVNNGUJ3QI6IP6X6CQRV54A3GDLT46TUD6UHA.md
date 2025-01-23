<!--- Autodesk.DesignScript.Geometry.TSpline.TSplineSurface.ByConePointsRadii --->
<!--- H54SEHAY3YGO3MOAVNNGUJ3QI6IP6X6CQRV54A3GDLT46TUD6UHA --->
## In-Depth
Nell'esempio seguente, viene creata la primitiva di un cono T-Spline utilizzando il nodo `TSplineSurface.ByConePointsRadii`. La posizione e l'altezza del cono sono controllate dai due input `startPoint` ed `endPoint`. I raggi di base e superiore possono essere regolati con gli input `startRadius` e `topRadius`. `radialSpans` e `heightSpans` determinano le campate radiali e di altezza. La simmetria iniziale della forma è specificata dall'input `symmetry`. Se la simmetria X o Y è impostata su True, il valore delle campate radiali deve essere un multiplo di 4. Infine, l'input `inSmoothMode` viene utilizzato per passare dall'anteprima in modalità uniforme a quella in modalità riquadro e viceversa della superficie T-Spline.

## File di esempio

![Example](./H54SEHAY3YGO3MOAVNNGUJ3QI6IP6X6CQRV54A3GDLT46TUD6UHA_img.jpg)
