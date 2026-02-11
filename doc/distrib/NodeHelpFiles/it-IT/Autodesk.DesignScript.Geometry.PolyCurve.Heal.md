## In profondità
`PolyCurve.Heal` utilizza una PolyCurve autointersecante e restituisce una nuova PolyCurve che non si autointerseca. La PolyCurve di input non può contenere più di 3 autointersezioni. In altre parole, se qualsiasi singolo segmento della PolyCurve incontra o interseca più di 2 segmenti, la correzione non funzionerà. Inserire un input `trimLength` maggiore di 0 e i segmenti finali più lunghi di `trimLength` non verranno tagliati.

Nell'esempio seguente, una PolyCurve autointersecante viene corretta utilizzando `PolyCurve.Heal`.
___
## File di esempio

![PolyCurve.Heal](./Autodesk.DesignScript.Geometry.PolyCurve.Heal_img.jpg)
