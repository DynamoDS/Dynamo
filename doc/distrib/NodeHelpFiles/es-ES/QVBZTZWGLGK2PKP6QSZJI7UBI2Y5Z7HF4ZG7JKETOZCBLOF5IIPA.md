<!--- Autodesk.DesignScript.Geometry.TSpline.TSplineSurface.DuplicateFaces --->
<!--- QVBZTZWGLGK2PKP6QSZJI7UBI2Y5Z7HF4ZG7JKETOZCBLOF5IIPA --->
## En detalle:
El nodo `TSplineSurface.DuplicateFaces` crea una nueva superficie de T-Spline compuesta únicamente de las caras copiadas seleccionadas.

En el ejemplo siguiente, se crea una superficie de T-Spline a través de `TSplineSurface.ByRevolve` mediante el uso de una curva NURBS como perfil.
A continuación, se selecciona un conjunto de caras de la superficie mediante `TSplineTopology.FaceByIndex`. Estas caras se duplican mediante `TSplineSurface.DuplicateFaces` y la superficie resultante se desplaza a un lado para obtener una mejor visualización.
___
## Archivo de ejemplo

![TSplineSurface.DuplicateFaces](./QVBZTZWGLGK2PKP6QSZJI7UBI2Y5Z7HF4ZG7JKETOZCBLOF5IIPA_img.jpg)
