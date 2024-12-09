## En detalle:
PlaneDeviation calculará primero el plano de ajuste óptimo a través de los puntos de un polígono especificado. A continuación, calcula la distancia de cada punto a ese plano para buscar la desviación máxima de los puntos desde el plano de ajuste óptimo. En el siguiente ejemplo, se genera una lista de ángulos, elevaciones y radios aleatorios y, a continuación, se utiliza Points.ByCylindricalCoordinates para crear un conjunto de puntos no planos que se utilizarán para Polygon.ByPoints. Al introducir este polígono en PlaneDeviation, se puede obtener la desviación media de los puntos desde un plano de ajuste óptimo.
___
## Archivo de ejemplo

![PlaneDeviation](./Autodesk.DesignScript.Geometry.Polygon.PlaneDeviation_img.jpg)

