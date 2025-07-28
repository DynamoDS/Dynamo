## En detalle:
Una superficie de T-Spline es estándar cuando todos los puntos T están separados de puntos de estrella por al menos dos isocurvas. La estandarización es necesaria para convertir una superficie de T-Spline en una superficie NURBS.

En el ejemplo siguiente, una superficie de T-Spline generada a través de `TSplineSurface.ByBoxLengths` tiene una de sus caras subdivididas. `TSplineSurface.IsStandard` se utiliza para comprobar si la superficie es estándar, pero genera un resultado negativo.
A continuación, se emplea `TSplineSurface.Standardize` para estandarizar la superficie. Se introducen nuevos puntos de control sin modificar la forma de la superficie. La superficie resultante se comprueba mediante `TSplineSurface.IsStandard`, que confirma que ahora es estándar.
Los nodos `TSplineFace.UVNFrame` y `TSplineUVNFrame.Position` se utilizan para resaltar la cara subdividida en la superficie.
___
## Archivo de ejemplo

![TSplineSurface.IsStandard](./Autodesk.DesignScript.Geometry.TSpline.TSplineSurface.IsStandard_img.jpg)
