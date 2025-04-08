## En detalle
`List.GroupBySimilarity` agrupa los elementos de la lista en función de la adyacencia de sus índices y la similitud de sus valores. La lista de elementos que se van a agrupar puede contener números (enteros y números de punto flotante) o cadenas, pero no una combinación de ambos.

Utilice la entrada `tolerance` para determinar la similitud de los elementos. Para las listas de números, el valor de `tolerance` representa la diferencia máxima admisible entre dos números para que se consideren similares.

Para las listas de cadenas, el valor de`tolerance`representa el número máximo de caracteres que pueden diferir entre dos cadenas utilizando la distancia de Levenshtein para la comparación. La tolerancia máxima para las cadenas está limitada a 10.

La entrada booleana `considerAdjacency` indica si se debe tener en cuenta la adyacencia al agrupar los elementos. Si se establece en "True" (verdadero), solo se agruparán los elementos adyacentes que sean similares. Si se establece en "False" (falso), solo se utilizará la similitud para formar clústeres, independientemente de la adyacencia.

El nodo genera una lista de listas de valores agrupados en función de la adyacencia y la similitud, así como una lista de listas de los índices de los elementos agrupados en la lista original.

En el ejemplo siguiente, `List.GroupBySimilarity` se utiliza de estas dos formas: para agrupar una lista de cadenas solo por similitud y para agrupar una lista de números por adyacencia y similitud.
___
## Archivo de ejemplo

![List.GroupBySimilarity](./DSCore.List.GroupBySimilarity_img.jpg)
