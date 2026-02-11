## En detalle
`GeometryColor.ByMeshColor` devuelve un objeto GeometryColor que es una malla coloreada según la lista de colores especificada. Hay un par de formas de utilizar este nodo:

- Si se especifica un color, toda la malla se colorea con un color determinado.
- Si el número de colores coincide con el número de triángulos, cada triángulo tiene el color correspondiente de la lista.
- Si el número de colores coincide con el número de vértices únicos, el color de cada triángulo en el color de la malla se interpola entre los valores de color en cada vértice.
- Si el número de colores es igual al número de vértices no únicos, el color de cada triángulo se interpola entre los valores de color de una cara, pero es posible que no se mezcle entre las caras.

## Ejemplo

En el ejemplo siguiente, una malla se codifica por colores en función de la elevación de sus vértices. En primer lugar, se utiliza `Mesh.Vertices` para obtener vértices de malla únicos que se analizan a continuación y se obtiene la elevación de cada punto de vértice mediante el nodo `Point.Z`. En segundo lugar, se utiliza `Map.RemapRange` para asignar los valores a un nuevo rango de 0 a 1 ajustando la escala de cada valor proporcionalmente. Por último, se utiliza `Color Range` para generar una lista de colores correspondientes con los valores asignados. Utilice esta lista de colores como la entrada `colors` del nodo `GeometryColor.ByMeshColors`. El resultado es una malla codificada por colores en la que el color de cada triángulo se interpola entre los colores de los vértices dando lugar a un degradado.

## Archivo de ejemplo

![Example](./Modifiers.GeometryColor.ByMeshColors_img.jpg)
