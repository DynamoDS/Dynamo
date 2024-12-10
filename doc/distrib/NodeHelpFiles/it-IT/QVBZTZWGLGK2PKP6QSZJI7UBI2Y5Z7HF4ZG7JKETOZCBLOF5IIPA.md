<!--- Autodesk.DesignScript.Geometry.TSpline.TSplineSurface.DuplicateFaces --->
<!--- QVBZTZWGLGK2PKP6QSZJI7UBI2Y5Z7HF4ZG7JKETOZCBLOF5IIPA --->
## In profondità
Il nodo `TSplineSurface.DuplicateFaces` crea una nuova superficie T-Spline composta solo dalle facce copiate selezionate.

Nell'esempio seguente, una superficie T-Spline viene creata tramite `TSplineSurface.ByRevolve`, utilizzando una curva NURBS come profilo.
Un gruppo di facce sulla superficie viene quindi selezionato utilizzando `TSplineTopology.FaceByIndex`. Queste facce vengono duplicate utilizzando `TSplineSurface.DuplicateFaces` e la superficie risultante viene spostata sul lato per una migliore visualizzazione.
___
## File di esempio

![TSplineSurface.DuplicateFaces](./QVBZTZWGLGK2PKP6QSZJI7UBI2Y5Z7HF4ZG7JKETOZCBLOF5IIPA_img.jpg)
