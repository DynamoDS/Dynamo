## En detalle:
`List.AllTrue` devuelve "False" (falso) si alguno de los elementos de la lista especificada es "False" (falso) o no es booleano. `List.AllTrue` solo devuelve "True" (verdadero) si todos los elementos de la lista especificada son booleanos y "True" (verdadero).

En el ejemplo siguiente, utilizamos `List.AllTrue` para evaluar listas de valores booleanos. La primera lista presenta un valor "False" (falso), por lo que se devuelve "False" (falso). La segunda lista solo presenta valores "True" (verdadero), por lo que se devuelve "True" (verdadero). La tercera lista presenta una sublista que incluye un valor "False" (falso), por lo que se devuelve "False" (falso). El nodo final evalúa las dos sublistas y devuelve "False" (falso) para la primera porque presenta un valor "False" (falso) y "True" (verdadero) para la segunda porque solo presenta valores "True" (verdadero).
___
## Archivo de ejemplo

![List.AllTrue](./DSCore.List.AllTrue_img.jpg)
