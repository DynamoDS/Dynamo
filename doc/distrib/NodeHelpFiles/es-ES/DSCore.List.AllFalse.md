## En detalle:
`List.AllFalse` devuelve "False" (falso) si alguno de los elementos de la lista especificada es "True" (verdadero) o no es booleano. `List.AllFalse` solo devuelve "True" (verdadero) si todos los elementos de la lista especificada son booleanos y "False" (falso).

En el ejemplo siguiente, utilizamos `List.AllFalse` para evaluar listas de valores booleanos. La primera lista presenta un valor "True" (verdadero), por lo que se devuelve "False" (falso). La segunda lista solo presenta valores "False" (falso), por lo que se devuelve "True" (verdadero). La tercera lista presenta una sublista que incluye un valor "True" (verdadero), por lo que se devuelve "False" (falso). El nodo final evalúa las dos sublistas y devuelve "False" (falso) para la primera porque presenta un valor "True" (verdadero) y "True" (verdadero) para la segunda porque solo presenta valores "False" (falso).
___
## Archivo de ejemplo

![List.AllFalse](./DSCore.List.AllFalse_img.jpg)
