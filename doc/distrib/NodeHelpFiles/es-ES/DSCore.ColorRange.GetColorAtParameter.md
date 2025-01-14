## En detalle:
GetColorAtParameter captura un rango de colores 2D de entrada y devuelve una lista de colores en los parámetros UV especificados en el rango de 0 a 1. En el siguiente ejemplo, se crea primero un rango de colores 2D mediante un nodo ByColorsAndParameters con una lista de colores y una lista de parámetros para establecer el rango. Se utiliza un bloque de código para generar un rango de números entre 0 y 1, que se utiliza como entradas U y V de un nodo UV.ByCoordinates. El encaje de este nodo se establece en Producto cartesiano. Se crea un conjunto de cubos de forma similar, con un nodo Point.ByCoordinates con un encaje establecido en Producto cartesiano para crear una matriz de cubos. A continuación, se utiliza un nodo Display.ByGeometryColor con la matriz de cubos y la lista de colores obtenidas del nodo GetColorAtParameter.
___
## Archivo de ejemplo

![GetColorAtParameter](./DSCore.ColorRange.GetColorAtParameter_img.jpg)

