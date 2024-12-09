## Em profundidade
`List.NormalizeDepth` retorna uma nova lista de profundidade uniforme para uma classificação ou profundidade de lista especificada.

Semelhante ao nó `List.Flatten`, é possível usar `List.NormalizeDepth` para retornar uma lista unidimensional (uma lista com um único nível). Mas também é possível usá-lo para adicionar níveis de lista. O nó normaliza a lista de entrada para uma profundidade de sua escolha.

No exemplo abaixo, uma lista contendo duas listas de profundidade desigual pode ser normalizada para diferentes classificações com um controle deslizante inteiro. Normalizando as profundidades em diferentes classificações, a lista aumenta ou diminui em profundidade, mas é sempre uniforme. Uma lista de classificação 1 retorna uma única lista de elementos, enquanto uma lista de classificação 3 retorna 2 níveis de sublistas.
___
## Arquivo de exemplo

![List.NormalizeDepth](./DSCore.List.NormalizeDepth_img.jpg)
