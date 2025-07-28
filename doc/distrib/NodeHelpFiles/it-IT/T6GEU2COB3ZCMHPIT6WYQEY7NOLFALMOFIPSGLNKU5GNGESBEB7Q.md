<!--- Autodesk.DesignScript.Geometry.NurbsCurve.ByControlPointsWeightsKnots --->
<!--- T6GEU2COB3ZCMHPIT6WYQEY7NOLFALMOFIPSGLNKU5GNGESBEB7Q --->
## In profondità
`NurbsCurve.ByControlPointsWeightsKnots` consente di controllare manualmente i pesi e i nodi di una NurbsCurve. L'elenco di pesi deve avere la stessa lunghezza dell'elenco di punti di controllo. La dimensione dell'elenco di nodi deve essere uguale al numero di punti di controllo più il grado più 1.

Nell'esempio seguente, viene innanzitutto creata una NurbsCurve interpolando tra una serie di punti casuali. Vengono utilizzati nodi, pesi e punti di controllo per trovare le parti corrispondenti di quella curva. È possibile utilizzare `List.ReplaceItemAtIndex` per modificare l'elenco di pesi. Infine, viene utilizzato `NurbsCurve.ByControlPointsWeightsKnots` per ricreare una NurbsCurve. con i pesi modificati.

___
## File di esempio

![ByControlPointsWeightsKnots](./T6GEU2COB3ZCMHPIT6WYQEY7NOLFALMOFIPSGLNKU5GNGESBEB7Q_img.jpg)

