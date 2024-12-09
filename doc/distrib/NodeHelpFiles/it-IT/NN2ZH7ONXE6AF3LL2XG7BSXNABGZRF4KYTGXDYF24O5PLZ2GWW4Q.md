<!--- Autodesk.DesignScript.Geometry.TSpline.TSplineSurface.EnableSmoothMode --->
<!--- NN2ZH7ONXE6AF3LL2XG7BSXNABGZRF4KYTGXDYF24O5PLZ2GWW4Q --->
## In profondità
La modalità riquadro e la modalità uniforme sono due metodi per visualizzare una superficie T-Spline. La modalità uniforme è la forma reale di una superficie T-Spline ed è utile per visualizzare l'anteprima delle caratteristiche estetiche e dimensionali del modello. La modalità riquadro, invece, permette di vedere la struttura della superficie e di comprenderla meglio, oltre ad essere un'opzione più rapida per visualizzare in anteprima la geometria grande o complessa. Il nodo `TSplineSurface.EnableSmoothMode` consente di passare da uno stato di anteprima all'altro in diverse fasi dello sviluppo della geometria.

Nell'esempio seguente, l'operazione di smussatura viene eseguita su una superficie del parallelepipedo T-Spline. Il risultato viene visualizzato in modalità riquadro (l'input `inSmoothMode` della superficie del parallelepipedo impostato su false) per una migliore comprensione della struttura della forma. La modalità uniforme viene quindi attivata tramite il nodo `TSplineSurface.EnableSmoothMode` e il risultato viene traslato a destra per visualizzare in anteprima entrambe le modalità contemporaneamente.
___
## File di esempio

![TSplineSurface.EnableSmoothMode](./NN2ZH7ONXE6AF3LL2XG7BSXNABGZRF4KYTGXDYF24O5PLZ2GWW4Q_img.jpg)
