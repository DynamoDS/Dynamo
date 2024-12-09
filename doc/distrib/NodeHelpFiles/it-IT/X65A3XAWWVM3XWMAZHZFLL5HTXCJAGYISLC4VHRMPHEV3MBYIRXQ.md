<!--- Autodesk.DesignScript.Geometry.Solid.BySweep(profile, path, cutEndOff) --->
<!--- X65A3XAWWVM3XWMAZHZFLL5HTXCJAGYISLC4VHRMPHEV3MBYIRXQ --->
## In profondità
`Solid.BySweep` crea un solido eseguendo l'estrusione su percorso di una curva di profilo chiusa di input lungo un percorso specificato.

Nell'esempio seguente, viene utilizzato un rettangolo come curva di profilo di base. Il percorso viene creato utilizzando una funzione coseno con una sequenza di angoli per variare le coordinate x di un insieme di punti. I punti vengono utilizzati come input in un nodo `NurbsCurve.ByPoints`. Viene quindi creato un solido eseguendo l'estrusione su percorso del rettangolo lungo la curva coseno creata.
___
## File di esempio

![Solid.BySweep](./X65A3XAWWVM3XWMAZHZFLL5HTXCJAGYISLC4VHRMPHEV3MBYIRXQ_img.jpg)
