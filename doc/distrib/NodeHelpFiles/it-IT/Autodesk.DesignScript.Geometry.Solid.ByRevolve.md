## In profondità
`Solid.ByRevolve` crea una superficie ruotando una determinata curva di profilo attorno ad un asse. L'asse viene definito da un punto `axisOrigin` e da un vettore `axisDirection`. L'angolo iniziale determina il punto dove iniziare la superficie, misurata in gradi, e `sweepAngle` determina la distanza attorno all'asse per continuare la superficie.

Nell'esempio seguente, viene utilizzata una curva generata con una funzione coseno come curva di profilo e due dispositivi di scorrimento numerici per controllare `startAngle` e `sweepAngle`. In `axisOrigin` e `axisDirection` vengono lasciati i valori di default dell'origine globale e dell'asse z globale per questo esempio.

___
## File di esempio

![ByRevolve](./Autodesk.DesignScript.Geometry.Solid.ByRevolve_img.jpg)

