## In profondità
Utilizzare `NurbsCurve.PeriodicKnots` quando è necessario esportare una curva NURBS chiusa in un altro sistema (ad esempio, Alias) o quando tale sistema richiede la curva nella sua forma periodica. Molti strumenti CAD richiedono questa forma per garantire l'accuratezza dei dati in entrata e in uscita.

`PeriodicKnots` restituisce il vettore del nodo nella forma *periodica* (non bloccata). `Knots` lo restituisce nella forma *bloccata*. Entrambe le matrici hanno la stessa lunghezza; sono due modi diversi di descrivere la stessa curva. Nella forma bloccata, i nodi vengono ripetuti all'inizio e alla fine in modo che la curva sia bloccata nell'intervallo di parametri. Nella forma periodica, la spaziatura dei nodi si ripete invece all'inizio e alla fine, creando un perimetro chiuso uniforme.

Nell'esempio seguente, viene creata una curva NURBS periodica con `NurbsCurve.ByControlPointsWeightsKnots`. I nodi Watch confrontano `Knots` e `PeriodicKnots` in modo da poter vedere la stessa lunghezza ma valori diversi. Knots è la forma bloccata (nodi ripetuti alle estremità) e PeriodicKnots è la forma non bloccata con il motivo di differenza ripetuto che definisce la periodicità della curva.
___
## File di esempio

![PeriodicKnots](./Autodesk.DesignScript.Geometry.NurbsCurve.PeriodicKnots_img.jpg)
