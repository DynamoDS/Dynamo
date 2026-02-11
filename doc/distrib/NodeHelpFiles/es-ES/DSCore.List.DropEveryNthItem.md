## En detalle:
`List.DropEveryNthItem` elimina elementos de la lista de entrada a intervalos del valor n de entrada. El punto inicial del intervalo se puede modificar con la entrada `offset`. Por ejemplo, si se introduce 3 en n y se deja el desfase con el valor por defecto 0, se eliminarán los elementos con los índices 2, 5, 8, etc. Con un desfase de 1, se eliminarán los elementos con los índices 0, 3, 6, etc. Tenga en cuenta que el desfase se "ajusta" en toda la lista. Para conservar los elementos seleccionados en lugar de eliminarlos, consulte `List.TakeEveryNthItem`.

En el ejemplo siguiente, generamos primero una lista de números con `Range` y, a continuación, eliminamos todos los demás números mediante el uso de 2 como entrada para `n`.
___
## Archivo de ejemplo

![List.DropEveryNthItem](./DSCore.List.DropEveryNthItem_img.jpg)
