<!--- Autodesk.DesignScript.Geometry.TSpline.TSplineSurface.SubdivideFaces --->
<!--- WKY3SVAE74IVMZW7MVT4F5TGIUFXAGA2W2FN6Q6PACG3NH6AMVFA --->
## In profondità
Nell'esempio seguente, viene generata una superficie T-Spline tramite il nodo `TSplineSurface.ByBoxLengths`.
Una faccia viene selezionata utilizzando il nodo `TSplineTopology.FaceByIndex` ed è suddivisa utilizzando il nodo `TSplineSurface.SubdivideFaces`.
Questo nodo divide le facce specificate in facce più piccole, quattro per le facce regolari, tre, cinque o più per NGons.
Quando l'input booleano per `exact` è impostato su true, il risultato è una superficie che tenta di mantenere la stessa forma esatta dell'originale durante l'aggiunta della suddivisione. È possibile aggiungere più isocurve per mantenere la forma. Quando è impostato su false, il nodo suddivide solo la faccia selezionata, il che spesso produce una superficie distinta dall'originale.
I nodi `TSplineFace.UVNFrame` e `TSplineUVNFrame.Position` vengono utilizzati per evidenziare il centro della faccia suddivisa.
___
## File di esempio

![TSplineSurface.SubdivideFaces](./WKY3SVAE74IVMZW7MVT4F5TGIUFXAGA2W2FN6Q6PACG3NH6AMVFA_img.jpg)
