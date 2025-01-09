## En detalle:
En el ejemplo siguiente, se investiga una superficie de T-Spline con reflexiones añadidas mediante el nodo `TSplineSurface.Reflections`, que devuelve una lista de todas las reflexiones aplicadas a la superficie. El resultado es una lista de dos reflexiones. A continuación, se transfiere la misma superficie a través de un nodo `TSplineSurface.RemoveReflections` y se inspecciona de nuevo. Esta vez, el nodo `TSplineSurface.Reflections` devuelve un error, debido a que se han eliminado las reflexiones.
___
## Archivo de ejemplo

![TSplineSurface.Reflections](./Autodesk.DesignScript.Geometry.TSpline.TSplineSurface.Reflections_img.jpg)
