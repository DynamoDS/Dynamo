## In profondità
Restituisce una nuova mesh con i seguenti difetti corretti:
- Componenti piccoli: se la mesh contiene segmenti molto piccoli (rispetto alle dimensioni complessive della mesh) e scollegati, questi verranno ignorati.
- Fori: i fori nella mesh vengono riempiti.
- Regioni non manifold: se un vertice è collegato a più di due bordi di *contorno* o un bordo è collegato a più di due triangoli, il vertice/bordo non è manifold. Mesh Toolkit rimuoverà la geometria fino a quando la mesh non sarà manifold.
Questo metodo tenta di conservare il più possibile la mesh originale, al contrario di MakeWatertight, che ricampiona la mesh.

Nell'esempio seguente, `Mesh.Repair` viene utilizzato su una mesh importata per riempire il buco attorno all'orecchio del coniglio.

## File di esempio

![Example](./Autodesk.DesignScript.Geometry.Mesh.Repair_img.jpg)
