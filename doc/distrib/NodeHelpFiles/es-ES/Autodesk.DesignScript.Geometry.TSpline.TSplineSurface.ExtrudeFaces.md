## En detalle:
En el ejemplo siguiente, se crea una superficie de T-Spline plana mediante `TSplineSurface.ByPlaneOriginNormal`, y se selecciona y se subdivide un conjunto de sus caras. A continuación, esas caras se extruyen simétricamente mediante el nodo `TSplineSurface.ExtrudeFaces` con la dirección (en este caso, el vector normal UVN de las caras) y el número de tramos especificados. Los bordes resultantes se desplazan en la dirección especificada.
___
## Archivo de ejemplo

![TSplineSurface.ExtrudeFaces](./Autodesk.DesignScript.Geometry.TSpline.TSplineSurface.ExtrudeFaces_img.jpg)
