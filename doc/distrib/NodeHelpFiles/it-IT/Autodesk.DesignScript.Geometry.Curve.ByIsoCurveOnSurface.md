## In profondità
Curve.ByIsoCurveOnSurface creerà una curva che è l'isocurva su una superficie specificando la direzione U o V e specificando il parametro nella direzione opposta in cui creare la curva. L'input "direction" determina la direzione dell'isocurva da creare. Un valore pari a 1 corrisponde alla direzione u, mentre un valore pari a 0 corrisponde alla direzione v. Nell'esempio seguente, viene prima creata la griglia di punti e poi traslata nella direzione Z di una quantità casuale. Questi punti vengono utilizzati per creare la superficie utilizzando un nodo NurbsSurface.ByPoints. Questa superficie viene utilizzata come baseSurface di un nodo ByIsoCurveOnSurface. Viene utilizzato un Number Slider impostato su un intervallo compreso tra 0 e 1 e un passo di 1 per controllare se si estrae l'isocurva nella direzione u o v. Viene utilizzato un secondo Number Slider per determinare il parametro in corrispondenza del quale viene estratta l'isocurva.
___
## File di esempio

![ByIsoCurveOnSurface](./Autodesk.DesignScript.Geometry.Curve.ByIsoCurveOnSurface_img.jpg)

