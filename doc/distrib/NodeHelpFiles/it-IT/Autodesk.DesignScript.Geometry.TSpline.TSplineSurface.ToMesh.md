## In profondità
Nell'esempio seguente, una semplice superficie del parallelepipedo T-Spline viene trasformata in una mesh utilizzando un nodo `TSplineSurface.ToMesh`. L'input `minSegments` definisce il numero minimo di segmenti per una faccia in ogni direzione ed è importante per il controllo della definizione della mesh. L'input `tolerance` corregge eventuali imprecisioni aggiungendo più posizioni dei vertici in modo che corrispondano alla superficie originale entro la tolleranza specificata. Il risultato è una mesh la cui definizione viene visualizzata in anteprima utilizzando un nodo `Mesh.VertexPositions`.
La mesh di output può contenere sia triangoli che quadrilateri, cosa importante da tenere presente se si utilizzano i nodi MeshToolkit.
___
## File di esempio

![TSplineSurface.ToMesh](./Autodesk.DesignScript.Geometry.TSpline.TSplineSurface.ToMesh_img.jpg)
