## En detalle:
`List.Chop` divide una determinada lista en listas más pequeñas basadas en una lista de longitudes de enteros de entrada. La primera lista anidada contiene el número de elementos especificados por el primer número en la entrada `lengths`. La segunda lista anidada contiene el número de elementos especificados por el segundo número en la entrada `lengths`, etc. `List.Chop` repite el último número de la entrada `lengths` hasta que se recorten todos los elementos de la lista de entrada.

En el ejemplo siguiente, utilizamos un bloque de código para generar un rango de números entre 0 y 5, escalonado en 1. Esta lista contiene seis elementos. Utilizamos un segundo bloque de código para crear una lista de longitudes en las que cortar la primera lista. El primer número de esta lista es 1, que utiliza `List.Chop` para crear una lista anidada con un solo elemento. El segundo número es 3, lo que crea una lista anidada con tres elementos. Como no se especifican más longitudes, `List.Chop` incluye todos los elementos restantes en la tercera y última lista.
___
## Archivo de ejemplo

![List.Chop](./DSCore.List.Chop_img.jpg)
