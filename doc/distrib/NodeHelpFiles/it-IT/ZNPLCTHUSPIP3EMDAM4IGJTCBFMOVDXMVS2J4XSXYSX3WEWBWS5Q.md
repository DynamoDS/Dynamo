<!--- Autodesk.DesignScript.Geometry.Curve.CoordinateSystemAtSegmentLength --->
<!--- ZNPLCTHUSPIP3EMDAM4IGJTCBFMOVDXMVS2J4XSXYSX3WEWBWS5Q --->
## In profondità
CoordinateSystemAtSegmentLength restituirà un sistema di coordinate allineato alla curva di input in corrispondenza della lunghezza della curva specificata, misurata dal punto iniziale della curva. Il sistema di coordinate risultante avrà il suo asse x nella direzione della normale della curva e l'asse y nella direzione della tangente della curva in corrispondenza della lunghezza specificata. Nell'esempio seguente, viene prima creata una curva NURBS utilizzando un nodo ByControlPoints, con un insieme di punti generati casualmente come input. Viene utilizzato un Number Slider per controllare l'input segmentLength per un nodo CoordinateSystemAtParameter. Se la lunghezza specificata è maggiore della lunghezza della curva, questo nodo restituirà un sistema di coordinate in corrispondenza del punto finale della curva.
___
## File di esempio

![CoordinateSystemAtSegmentLength](./ZNPLCTHUSPIP3EMDAM4IGJTCBFMOVDXMVS2J4XSXYSX3WEWBWS5Q_img.jpg)

