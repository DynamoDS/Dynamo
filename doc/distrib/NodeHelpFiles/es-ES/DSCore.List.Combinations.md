## En detalle:
`List.Combinations` devuelve una lista anidada que incluye todas las combinaciones posibles de los elementos de la lista de entrada con la longitud especificada. En las combinaciones, el orden de los elementos es irrelevante, por lo que la lista de salida (0,1) se considera la misma combinación que (1,0). Si `replace` se establece en "True" (verdadero), los elementos se reemplazarán en la lista original, lo que permite utilizarlos repetidamente en una combinación.

En el ejemplo siguiente, utilizamos un bloque de código para generar un rango de números de 0 a 5, escalonado en 1. Utilizamos `List.Combinations` con una longitud de entrada de 3 para generar todas las formas diferentes de combinar tres de los números del rango. El valor booleano `replace` se establece en "True" (verdadero), por lo que los números se utilizarán repetidamente.
___
## Archivo de ejemplo

![List.Combinations](./DSCore.List.Combinations_img.jpg)
