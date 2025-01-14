## En detalle:
`List.FilterByBoolMask` utiliza dos listas como entradas. La primera lista se divide en dos listas independientes de acuerdo con una lista correspondiente de valores booleanos ("True" o "False"). Los elementos de la entrada `list` que se corresponden con el valor "True" (verdadero) en la entrada `mask` se dirigen a la salida etiquetada como "In", mientras que los elementos que se corresponden con el valor "False" (falso) se dirigen a la salida etiquetada como `out`.

En el ejemplo siguiente, `List.FilterByBoolMask` se utiliza para elegir madera y laminado en una lista de materiales. En primer lugar, comparamos dos listas para buscar elementos coincidentes y, a continuación, utilizamos un operador `Or` para comprobar los elementos con el valor "True" (verdadero) de la lista. A continuación, los elementos de la lista se filtran en función de si son de madera o laminado, o de cualquier otro material.
___
## Archivo de ejemplo

![List.FilterByBoolMask](./DSCore.List.FilterByBoolMask_img.jpg)
