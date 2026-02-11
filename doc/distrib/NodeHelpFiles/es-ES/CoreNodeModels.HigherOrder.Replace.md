## En detalle:
ReplaceByCondition utilizará una lista especificada y evaluará cada elemento con una condición indicada. Si la condición se evalúa como "true" (verdadera), el elemento correspondiente se reemplazará en la lista de salida por el elemento especificado en la entrada replaceWith. En el siguiente ejemplo, se utiliza un nodo Formula y se introduce la fórmula "x%2==0", que busca el resto de un elemento especificado después de dividirlo por 2; a continuación, comprueba si el resto es igual a 0. Esta fórmula devolverá "true" (verdadero) para enteros pares. Tenga en cuenta que la entrada x se deja en blanco. Si se utiliza esta fórmula como condición en un nodo ReplaceByCondition, se genera una lista de salida donde cada número par se reemplaza por el elemento especificado, en este caso, el entero 10.
___
## Archivo de ejemplo

![ReplaceByCondition](./CoreNodeModels.HigherOrder.Replace_img.jpg)

