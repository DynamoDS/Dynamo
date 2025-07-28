## En detalle:
`List.TakeEveryNthItem` genera una nueva lista que contiene solo los elementos de la lista de entrada que se encuentran a intervalos del valor n de entrada. El punto inicial del intervalo se puede cambiar con la entrada `offset`. Por ejemplo, si se introduce 3 en n y se deja el desfase con el valor por defecto 0, se mantendrán los elementos con índices 2, 5, 8, etc. Con un desfase de 1, se mantendrán los elementos con índices 0, 3, 6, etc. Tenga en cuenta que el desfase "ajusta" toda la lista. Para eliminar los elementos seleccionados en lugar de guardarlos, consulte `List.DropEveryNthItem`.

En el ejemplo siguiente, generamos primero una lista de números con `Range` y, a continuación, conservamos cada dos números usando 2 como entrada para n.
___
## Archivo de ejemplo

![List.TakeEveryNthItem](./DSCore.List.TakeEveryNthItem_img.jpg)
