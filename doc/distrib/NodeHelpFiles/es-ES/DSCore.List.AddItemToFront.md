## En detalle:
`List.AddItemToFront` inserta el elemento especificado al inicio de una determinada lista. El nuevo elemento presenta el índice 0, mientras que los elementos originales se desplazan todos por un índice de 1. Tenga en cuenta que, si el elemento que se va a añadir es una lista, esta se agrega como un único objeto, lo que genera una lista anidada. Para combinar dos listas en una única lista plana, consulte `List.Join`.

En el ejemplo siguiente, se utiliza un bloque de código para generar un rango de números de 0 a 5, escalonado en 1. A continuación, se añade un nuevo elemento, el número 20, al principio de esa lista mediante `List.AddItemToFront`.
___
## Archivo de ejemplo

![List.AddItemToFront](./DSCore.List.AddItemToFront_img.jpg)
