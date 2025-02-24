## En detalle:
`List.GroupByFunction` devuelve una nueva lista agrupada por una función.

La entrada `groupFunction` requiere un nodo con un estado de función (es decir, devuelve una función). Esto significa que al menos una de las entradas del nodo no está conectada. Dynamo ejecuta la función del nodo en cada elemento de la lista de entrada de `List.GroupByFunction` para utilizar la salida como un mecanismo de agrupación.

En el ejemplo siguiente, se agrupan dos listas diferentes utilizando `List.GetItemAtIndex` como función. Esta crea grupos (una nueva lista) a partir de cada índice de nivel superior.
___
## Archivo de ejemplo

![List.GroupByFunction](./List.GroupByFunction_img.jpg)
