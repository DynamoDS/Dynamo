## In-Depth
Nell'esempio seguente, viene creata la primitiva di un cono T-Spline utilizzando il nodo `TSplineSurface.ByConePointsRadius`. La posizione e l'altezza del cono sono controllate dai due input `startPoint` ed `endPoint`. È possibile regolare solo il raggio di base con l'input `radius` e il raggio superiore è sempre zero.`radialSpans` e `heightSpans` determinano le campate di raggio e altezza. La simmetria iniziale della forma è specificata dall'input `symmetry`. Se la simmetria X o Y è impostata su True, il valore delle campate radiali deve essere un multiplo di 4. Infine, l'input `inSmoothMode` viene utilizzato per passare dall'anteprima in modalità uniforme a quella in modalità riquadro e viceversa della superficie T-Spline.

## File di esempio

![Example](./GVO3NNSNHNAH3DJS5OR37DI2A457QGYX4BQGMHO4IGUUUHZV3HSQ_img.jpg)
