## In profondità
Curve.ByParameterLineOnSurface creerà una linea lungo una superficie tra due coordinate UV di input. Nell'esempio seguente, viene prima creata una griglia di punti, che viene poi traslata nella direzione Z di un valore casuale. Questi punti vengono utilizzati per creare la superficie utilizzando un nodo NurbsSurface.ByPoints. Questa superficie viene utilizzata come baseSurface di un nodo ByParameterLineOnSurface. Viene utilizzato un insieme di Number Slider per regolare gli input U e V di due nodi UV.ByCoordinates, che sono quindi usati per determinare il punto iniziale e finale della linea sulla superficie.
___
## File di esempio

![ByParameterLineOnSurface](./Autodesk.DesignScript.Geometry.Curve.ByParameterLineOnSurface_img.jpg)

