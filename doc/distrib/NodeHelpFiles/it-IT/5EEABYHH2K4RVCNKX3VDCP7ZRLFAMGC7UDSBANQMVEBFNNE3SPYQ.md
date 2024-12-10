<!--- Autodesk.DesignScript.Geometry.Curve.NormalAtParameter(curve, param) --->
<!--- 5EEABYHH2K4RVCNKX3VDCP7ZRLFAMGC7UDSBANQMVEBFNNE3SPYQ --->
## In profondità
`Curve.NormalAtParameter (curve, param)` restituisce un vettore allineato con la direzione normale in corrispondenza del parametro specificato di una curva. La parametrizzazione di una curva viene misurata nell'intervallo compreso tra 0 e 1, con 0 che rappresenta l'inizio della curva e 1 che rappresenta la fine della curva.

Nell'esempio seguente, viene innanzitutto creata una NurbsCurve utilizzando un nodo `NurbsCurve.ByControlPoints`, con un insieme di punti generati in modo casuale come input. Viene utilizzato un dispositivo di scorrimento numerico impostato sull'intervallo da 0 a 1 per controllare l'input `parameter` per un nodo `Curve.NormalAtParameter`.
___
## File di esempio

![Curve.NormalAtParameter(curve, param](./5EEABYHH2K4RVCNKX3VDCP7ZRLFAMGC7UDSBANQMVEBFNNE3SPYQ_img.jpg)
