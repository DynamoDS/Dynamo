## En detalle
`TSplineInitialSymmetry.ByAxial` define si la geometría de T-Spline presenta simetría a lo largo de un eje elegido (x, y, z). La simetría puede darse en uno, dos o los tres ejes. Una vez establecida en la creación de la geometría de T-Spline, la simetría influye en todas las operaciones y modificaciones posteriores.

En el ejemplo siguiente, se utiliza `TSplineSurface.ByBoxCorners` para crear una superficie de T-Spline. Entre las entradas de este nodo, `TSplineInitialSymmetry.ByAxial` se utiliza para definir la simetría inicial en la superficie. A continuación, se utilizan `TSplineTopology.RegularFaces` y `TSplineSurface.ExtrudeFaces` para seleccionar y extruir respectivamente una cara de la superficie de T-Spline. A continuación, la operación de extrusión se refleja alrededor de los ejes de simetría definidos con el nodo `TSplineInitialSymmetry.ByAxial`.

## Archivo de ejemplo

![Example](./Autodesk.DesignScript.Geometry.TSpline.TSplineInitialSymmetry.ByAxial_img.gif)
