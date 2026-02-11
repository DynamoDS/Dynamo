## En detalle:
`List.Transpose` intercambia las filas y las columnas de una lista de listas. Por ejemplo, una lista que contiene 5 sublistas de 10 elementos; cada una se transpondría a 10 listas de 5 elementos. Se insertan valores nulos según sea necesario para garantizar que cada sublista tenga el mismo número de elementos.

En el ejemplo, generamos una lista de números de 0 a 5 y otra lista de letras de A a E. A continuación, se utiliza `List.Create` para combinarlas. `List.Transpose` genera seis listas de dos elementos cada una, un número y una letra por lista. Observe que, como una de las listas originales era más larga que la otra, `List.Transpose` ha insertado un valor nulo para el elemento sin emparejar.
___
## Archivo de ejemplo

![List.Transpose](./DSCore.List.Transpose_img.jpg)
