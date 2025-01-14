## En detalle:
`List.RemoveIfNot` devuelve una lista que conserva los elementos que coinciden con el tipo de elemento especificado y elimina todos los demás elementos de la lista original.

En la entrada `type`, es posible que deba utilizar la ruta completa del nodo, como `Autodesk.DesignScript.Geometry.Surface` para eliminar elementos. Para recuperar las rutas de los elementos de la lista, puede introducir la lista en un nodo `Object.Type`.

En el ejemplo siguiente, `List.RemoveIfNot` devuelve una lista con una línea, eliminando los elementos de punto de la lista original porque no coinciden con el tipo especificado.
___
## Archivo de ejemplo

![List.RemoveIfNot](./List.RemoveIfNot_img.jpg)
