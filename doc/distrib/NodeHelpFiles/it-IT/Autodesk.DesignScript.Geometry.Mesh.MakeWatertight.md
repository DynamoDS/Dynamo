## In profondità
`Mesh.MakeWatertight` genera una mesh solida, a tenuta stagna e stampabile in 3D campionando la mesh originale. Fornisce un modo rapido per risolvere numerosi problemi di una mesh come autointersezioni, sovrapposizioni e geometria non manifold. Il metodo calcola un campo di distanza a banda sottile e genera una nuova mesh utilizzando l'algoritmo dei marching cubes, ma non viene proiettato nuovamente sulla mesh originale. Questa opzione è più adatta agli oggetti mesh che presentano numerosi difetti o problemi difficili quali le autointersezioni.
L'esempio seguente mostra un vaso non a tenuta stagna e il suo equivalente a tenuta ermetica.

## File di esempio

![Example](./Autodesk.DesignScript.Geometry.Mesh.MakeWatertight_img.jpg)
