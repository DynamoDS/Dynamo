## In profondità
HorizontalFrameAtParameter restituirà un sistema di coordinate allineato alla curva di input in corrispondenza del parametro specificato. La parametrizzazione di una curva viene misurata nell'intervallo compreso tra 0 e 1, con 0 che rappresenta l'inizio della curva e 1 che rappresenta la fine della curva. Il sistema di coordinate risultante avrà l'asse Z nella direzione Z globale e l'asse y nella direzione della tangente della curva in corrispondenza del parametro specificato. Nell'esempio seguente, viene prima creata una curva NURBS utilizzando un nodo ByControlPoints, con un insieme di punti generati in modo casuale come input. Viene utilizzato un Number Slider impostato sull'intervallo compreso tra 0 e 1 per controllare l'input del parametro per un nodo HorizontalFrameAtParameter.
___
## File di esempio

![HorizontalFrameAtParameter](./Autodesk.DesignScript.Geometry.Curve.HorizontalFrameAtParameter_img.jpg)

