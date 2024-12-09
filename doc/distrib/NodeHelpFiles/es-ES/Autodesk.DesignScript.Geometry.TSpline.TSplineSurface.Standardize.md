## En detalle:
El nodo `TSplineSurface.Standardize` se utiliza para estandarizar una superficie de T-Spline.
La estandarización consiste en preparar una superficie de T-Spline para la conversión a NURBS e implica extender todos los puntos T hasta que estén separados de los puntos de estrella por al menos dos isocurvas. El proceso de estandarización no cambia la forma de la superficie, pero puede añadir puntos de control para cumplir los requisitos de geometría necesarios para que la superficie sea compatible con NURBS.

En el ejemplo siguiente, una superficie de T-Spline generada a través de `TSplineSurface.ByBoxLengths` tiene una de sus caras subdivididas.
Se utiliza un nodo `TSplineSurface.IsStandard` para comprobar si la superficie es estándar, pero genera un resultado negativo.
A continuación, se emplea `TSplineSurface.Standardize` para estandarizar la superficie. La superficie resultante se comprueba mediante `TSplineSurface.IsStandard`, que confirma que ahora es estándar.
The nodes `TSplineFace.UVNFrame` and `TSplineUVNFrame.Position` are used to highlight the subdivided face in the surface.
___
## Archivo de ejemplo

![TSplineSurface.Standardize](./Autodesk.DesignScript.Geometry.TSpline.TSplineSurface.Standardize_img.jpg)
