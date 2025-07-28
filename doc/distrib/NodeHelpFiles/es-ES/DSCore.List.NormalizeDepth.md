## En detalle:
`List.NormalizeDepth` devuelve una nueva lista de profundidad uniforme a una clasificación o una profundidad de lista especificadas.

Al igual que `List.Flatten`, puede utilizar `List.NormalizeDepth` para devolver una lista unidimensional (una lista con un solo nivel). No obstante, también puede utilizar este nodo para añadir niveles de lista. Este normaliza la lista de entrada hasta la profundidad que elija.

En el ejemplo siguiente, una lista que contiene dos listas de profundidad desigual se puede normalizar con diferentes clasificaciones con un control deslizante de entero. Al normalizar las profundidades en diferentes clasificaciones, la lista aumenta o disminuye en profundidad, pero siempre es uniforme. Una lista de clasificación 1 devuelve una única lista de elementos, mientras que una lista de clasificación 3 devuelve dos niveles de sublistas.
___
## Archivo de ejemplo

![List.NormalizeDepth](./DSCore.List.NormalizeDepth_img.jpg)
