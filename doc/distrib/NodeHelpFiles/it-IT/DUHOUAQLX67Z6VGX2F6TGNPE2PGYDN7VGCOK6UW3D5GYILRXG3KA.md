<!--- Autodesk.DesignScript.Geometry.Curve.SweepAsSurface(curve, path, cutEndOff) --->
<!--- DUHOUAQLX67Z6VGX2F6TGNPE2PGYDN7VGCOK6UW3D5GYILRXG3KA --->
## In profondità
`Curve.SweepAsSurface` creerà una superficie eseguendo l'estrusione su percorso di una curva di input lungo una traiettoria specificata. Nell'esempio seguente, viene creata una curva di cui eseguire l'estrusione su percorso utilizzando un Code Block per creare tre punti di un nodo `Arc.ByThreePoints`. Viene creata una curva della traiettoria da una linea semplice lungo l'asse x. `Curve.SweepAsSurface` sposta la curva di profilo lungo la curva della traiettoria creando una superficie. Il parametro `cutEndOff` è un valore booleano che controlla il trattamento di estremità della superficie di estrusione su percorso. Se impostato su `true`, le estremità della superficie vengono tagliate perpendicolarmente (normale) alla curva della traiettoria, producendo terminazioni pulite e piatte. Se impostato su `false` (valore di default), le estremità della superficie seguono la forma naturale della curva di profilo senza alcun taglio, il che può comportare estremità angolate o irregolari a seconda della curvatura del percorso.
___
## File di esempio

![Example](./DUHOUAQLX67Z6VGX2F6TGNPE2PGYDN7VGCOK6UW3D5GYILRXG3KA_img.jpg)

