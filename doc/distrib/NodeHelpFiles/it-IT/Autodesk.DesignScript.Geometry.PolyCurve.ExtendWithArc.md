## In profondità
ExtendWithArc aggiungerà un arco circolare all'inizio o alla fine di una PolyCurve di input e restituirà una singola PolyCurve combinata. L'input radius determinerà il raggio del cerchio, mentre l'input length determina la distanza lungo il cerchio per l'arco. La lunghezza totale deve essere minore di o uguale alla lunghezza di un cerchio completo con il raggio specificato. L'arco generato sarà tangente alla fine della PolyCurve di input. Un input booleano per endOrStart controlla in quale estremità della PolyCurve verrà creato l'arco. Un valore "true" determina l'arco creato alla fine della PolyCurve, mentre "false" creerà l'arco all'inizio della PolyCurve. Nell'esempio seguente, si utilizza prima un insieme di punti casuali e PolyCurve.ByPoints per generare una PolyCurve. Si utilizzano quindi due Number Slider e un pulsante di commutazione booleano per impostare i parametri per ExtendWithArc.
___
## File di esempio

![ExtendWithArc](./Autodesk.DesignScript.Geometry.PolyCurve.ExtendWithArc_img.jpg)

