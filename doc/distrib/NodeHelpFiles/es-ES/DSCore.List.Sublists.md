## En detalle:
`List.Sublists` devuelve una serie de sublistas de la lista especificada en función del rango y el desfase de entrada. El rango determina los elementos de la lista de entrada que se colocan en la primera sublista. Se aplica un desfase al rango y el nuevo rango determina la segunda sublista. Este proceso se repite, aumentando el índice inicial del rango según el desfase especificado hasta que la sublista resultante esté vacía.

En el ejemplo siguiente, empezamos con un rango de números de 0 a 9. El rango de 0 a 5 se utiliza como rango de sublista con un desfase de 2. En la salida de sublistas anidadas, la primera lista contiene los elementos con índices en el rango de 0 a 5 y la segunda lista contiene los elementos con índices de 2 a 7. Al repetirse, las sublistas posteriores se acortan a medida que el final del rango supera la longitud de la lista inicial.
___
## Archivo de ejemplo

![List.Sublists](./DSCore.List.Sublists_img.jpg)
