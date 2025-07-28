## En detalle:
`List.UniqueItems` elimina todos los elementos duplicados de una lista de entrada mediante la creación de una nueva lista que incluye únicamente los elementos que solo aparecen una vez en la lista original.

En el ejemplo siguiente, utilizamos `Math.RandomList` para generar primero una lista de números aleatorios entre 0 y 1. A continuación, los multiplicamos por 10 y utilizamos una operación `Math.Floor` para devolver una lista de enteros aleatorios entre 0 y 9, muchos de ellos repetidos varias veces. Utilizamos `List.UniqueItems` para crear una lista en la que cada entero solo aparezca una vez. El orden de la lista de salida se basa en el primer ejemplar encontrado de un elemento.
___
## Archivo de ejemplo

![List.UniqueItems](./DSCore.List.UniqueItems_img.jpg)
