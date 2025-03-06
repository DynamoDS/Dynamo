## En detalle:
En el ejemplo siguiente, una superficie de T-Spline deja de ser válida, lo que puede observarse al aparecer caras superpuestas en la vista preliminar del fondo. El hecho de que la superficie no sea válida puede confirmarse por un fallo en la activación del modo de suavizado mediante el nodo `TSplineSurface.EnableSmoothMode`. Otra pista es que el nodo `TSplineSurface.IsInBoxMode` devuelve `true` (verdadero), aunque la superficie tenga activada inicialmente el modo de suavizado.

Para reparar la superficie, se transfiere a través de un nodo `TSplineSurface.Repair`. El resultado es una superficie válida, que se puede confirmar activando correctamente el modo de suavizado de vista preliminar.
___
## Archivo de ejemplo

![TSplineSurface.Repair](./Autodesk.DesignScript.Geometry.TSpline.TSplineSurface.Repair_img.jpg)
