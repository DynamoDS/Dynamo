<!--- Autodesk.DesignScript.Geometry.TSpline.TSplineSurface.ByTorusCoordinateSystemRadii --->
<!--- TTAJ2WGGNFLM755ADOCD3G7N4MJBQI66CAC7SXM3XCYLEIPLBOCQ --->
## In-Depth
Nell'esempio seguente, viene creata una superficie del toro T-Spline con origine in corrispondenza di un sistema di coordinate specifico `cs`. I raggi minore e maggiore della forma vengono impostati dagli input `innerRadius` e `outerRadius`. I valori di `innerSpans` e `outerRadiusSpans` controllano la definizione della superficie lungo le due direzioni. La simmetria iniziale della forma è specificata dall'input `symmetry`. Se la simmetria assiale applicata alla forma è attiva per l'asse X o Y, il valore di `outerRadiusSpans` del toro deve essere un multiplo di 4. La simmetria radiale non ha tale requisito. Infine, l'input `inSmoothMode` viene utilizzato per passare dall'anteprima in modalità uniforme a quella in modalità riquadro e viceversa della superficie T-Spline.

## File di esempio

![Example](./TTAJ2WGGNFLM755ADOCD3G7N4MJBQI66CAC7SXM3XCYLEIPLBOCQ_img.jpg)
