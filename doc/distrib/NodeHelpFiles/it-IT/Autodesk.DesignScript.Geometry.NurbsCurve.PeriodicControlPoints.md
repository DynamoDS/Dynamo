## In profondità
Utilizzare `NurbsCurve.PeriodicControlPoints` quando è necessario esportare una curva NURBS chiusa in un altro sistema (ad esempio, Alias) o quando tale sistema richiede la curva nella sua forma periodica. Molti strumenti CAD richiedono questa forma per garantire l'accuratezza dei dati in entrata e in uscita.

`PeriodicControlPoints` restituisce i punti di controllo nella forma *periodica*. `ControlPoints` li restituisce nella forma *bloccata*. Entrambe le matrici hanno lo stesso numero di punti; sono due modi diversi di descrivere la stessa curva. Nella forma periodica, gli ultimi punti di controllo corrispondono ai primi (tanti quanti i gradi della curva), in modo che la curva si chiuda in modo uniforme. La forma bloccata utilizza un layout diverso, pertanto le posizioni dei punti nelle due matrici differiscono.

Nell'esempio seguente, viene creata una curva NURBS periodica con `NurbsCurve.ByControlPointsWeightsKnots`. I nodi Watch confrontano `ControlPoints` e `PeriodicControlPoints` in modo da poter vedere la stessa lunghezza ma posizioni di punti diverse. ControlPoints sono visualizzati in rosso, in modo che appaiano distintamente da PeriodicControlPoints, che sono in nero, nell'anteprima di sfondo.
___
## File di esempio

![PeriodicControlPoints](./Autodesk.DesignScript.Geometry.NurbsCurve.PeriodicControlPoints_img.jpg)
