<!--- Autodesk.DesignScript.Geometry.Curve.TrimSegmentsByParameter(curve, parameters, discardEvenSegments) --->
<!--- BZCTQI2SIMCNMSCEHGSQLE6G74ND4ZQRICVGQCLVQ3OGHPBNX5NQ --->
## In profondità
`Curve.TrimSegmentsByParameter (parameters, discardEvenSegments)` innanzitutto divide una curva in corrispondenza dei punti determinati da un elenco di input di parametri. Restituisce quindi i segmenti a numeri dispari o i segmenti a numeri pari, come determinato dal valore booleano dell'input `discardEvenSegments`.

Nell'esempio seguente, viene innanzitutto creata una NurbsCurve utilizzando un nodo `NurbsCurve.ByControlPoints`, con un insieme di punti generati in modo casuale come input. Viene utilizzato un input `code block` per creare un intervallo di numeri compreso tra 0 e 1, con incrementi di 0.1. Utilizzando questo come parametri di input per un nodo `Curve.TrimSegmentsByParameter` si crea un elenco di curve che sono una versione a linee tratteggiate della curva originale.
___
## File di esempio

![Curve.TrimSegmentsByParameter(parameters, discardEvenSegments)](./BZCTQI2SIMCNMSCEHGSQLE6G74ND4ZQRICVGQCLVQ3OGHPBNX5NQ_img.jpg)
