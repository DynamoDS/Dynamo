<!--- Autodesk.DesignScript.Geometry.Curve.SweepAsSolid(curve, path, cutEndOff) --->
<!--- LUVHU25JWECNEBKIBZFH6N5EUAM42XM3BSEOTMCI3TQDNS5EKLXA --->
## In profondità
`Curve.SweepAsSolid` crea un solido eseguendo l'estrusione su percorso di una curva di profilo chiusa di input lungo un percorso specificato.

Nell'esempio seguente, viene utilizzato un rettangolo come curva di profilo di base. Il percorso viene creato utilizzando una funzione coseno con una sequenza di angoli per variare le coordinate x di un insieme di punti. I punti vengono utilizzati come input in un nodo `NurbsCurve.ByPoints`. Viene quindi creato un solido eseguendo l'estrusione su percorso del rettangolo lungo la curva coseno creata con un nodo `Curve.SweepAsSolid`.
___
## File di esempio

![Curve.SweepAsSolid(curve, path, cutEndOff)](./LUVHU25JWECNEBKIBZFH6N5EUAM42XM3BSEOTMCI3TQDNS5EKLXA_img.jpg)
