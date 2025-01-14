## En detalle:
Polygon.Center busca el centro de un polígono especificado utilizando el valor medio de las esquinas. En los polígonos cóncavos, es posible que el centro se encuentre realmente fuera del polígono. En el siguiente ejemplo, se genera primero una lista de ángulos y radios aleatorios para utilizarlos como entradas para Point.ByCylindricalCoordinates. Al ordenar primero los ángulos, se garantiza que el polígono resultante se conectará en orden de ángulo creciente y, por lo tanto, no se intersecará consigo mismo. A continuación, se puede utilizar Center para obtener la media de los puntos y buscar el centro del polígono.
___
## Archivo de ejemplo

![Center](./Autodesk.DesignScript.Geometry.Polygon.Center_img.jpg)

