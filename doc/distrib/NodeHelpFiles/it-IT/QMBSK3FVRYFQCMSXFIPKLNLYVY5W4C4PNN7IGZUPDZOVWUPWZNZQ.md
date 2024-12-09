<!--- Autodesk.DesignScript.Geometry.TSpline.TSplineSurface.ByPlaneBestFitThroughPoints --->
<!--- QMBSK3FVRYFQCMSXFIPKLNLYVY5W4C4PNN7IGZUPDZOVWUPWZNZQ --->
## In-Depth
`TSplineSurface.ByPlaneBestFitThroughPoints` genera una superficie del piano della primitiva T-Spline da un elenco di punti. Per creare il piano T-Spline, il nodo utilizza i seguenti input:
- `points`: un gruppo di punti per definire l'orientamento e l'origine del piano. Nei casi in cui i punti di input non si trovino su un singolo piano, l'orientamento del piano viene determinato in base all'adattamento. Per creare la superficie, è necessario un minimo di tre punti.
- `minCorner` e `maxCorner`: gli angoli del piano, rappresentati come punti con valori X e Y (le coordinate Z verranno ignorate). Questi angoli rappresentano le estensioni della superficie T-Spline di output se viene traslata nel piano XY. I punti `minCorner` e `maxCorner` non devono coincidere con i vertici degli angoli in 3D.
- `xSpans` and `ySpans`: number of width and length spans/divisions of the plane
- `symmetry`: whether the geometry is symmetrical with respect to its X, Y and Z axes
- `inSmoothMode`: whether the resulting geometry will appear with smooth or box mode

Nell'esempio seguente, viene creata una superficie T-Spline piana utilizzando un elenco di punti generato in modo casuale. La dimensione della superficie è controllata dai due punti utilizzati come input `minCorner` e `maxCorner`.

## File di esempio

![Example](./QMBSK3FVRYFQCMSXFIPKLNLYVY5W4C4PNN7IGZUPDZOVWUPWZNZQ_img.jpg)
