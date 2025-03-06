## In profondità
`PolySurface.BySweep (rail, crossSection)` restituisce una PolySurface eseguendo l'estrusione su percorso di un elenco di linee connesse non intersecanti lungo una guida. L'input `crossSection` può ricevere un elenco di curve connesse che devono incontrarsi in un punto iniziale o finale oppure il nodo non restituirà una PolySurface. Questo nodo è simile a `PolySurface.BySweep (rail, profile)`; l'unica differenza è che l'input `crossSection` utilizza un elenco di curve, mentre `profile` utilizza solo una curva.

Nell'esempio seguente, viene creata una PolySurface tramite l'estrusione su percorso lungo un arco.


___
## File di esempio

![PolySurface.BySweep](./Autodesk.DesignScript.Geometry.PolySurface.BySweep_img.jpg)
