## Em profundidade
Os clusters `List.GroupBySimilarity` listam elementos com base na adjacência de seus índices e na semelhança de seus valores. A lista dos elementos a serem agrupados pode conter números (números inteiros e números de ponto flutuante) ou sequências de caracteres, mas não uma combinação dos dois.

Use a entrada `tolerance` para determinar a similaridade dos elementos. Para listas de números, o valor de 'tolerance' representa a diferença máxima permitida entre dois números para que sejam considerados semelhantes.

Para listas de sequências de caracteres, 'tolerance' representa o número máximo de caracteres que podem diferir entre duas sequências de caracteres, usando a distância de Levenshtein para comparação. A tolerância máxima para as sequências de caracteres é definida como 10.

A entrada booleana `considerAdjacency` indica se a adjacência deve ser considerada ao agrupar os elementos. Se True, somente os elementos adjacentes que são similares serão agrupados. Se False, somente a similaridade será usada para formar clusters, independentemente da adjacência.

O nó gera uma lista de listas de valores agrupados com base em adjacência e similaridade, bem como uma lista de listas dos índices dos elementos agrupados na lista original.

No exemplo abaixo, `List.GroupBySimilarity` é usado de duas maneiras: para agrupar uma lista de sequências de caracteres somente por similaridade e para agrupar uma lista de números por adjacência e similaridade.
___
## Arquivo de exemplo

![List.GroupBySimilarity](./DSCore.List.GroupBySimilarity_img.jpg)
