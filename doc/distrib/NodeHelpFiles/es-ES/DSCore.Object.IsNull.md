## En detalle:
IsNull devolverá un valor booleano en función de si un objeto es nulo. En el siguiente ejemplo, se dibuja una rejilla de círculos con radios variables en función del nivel rojo de un mapa de bits. Si no hay ningún valor rojo, no se dibuja ningún círculo y se devuelve "null" (nulo) en la lista de círculos. Al transferir esta lista a través de IsNull, se devuelve una lista de valores booleanos, donde "true" (verdadero) representa cada ubicación de un valor nulo. Esta lista de valores booleanos se puede utilizar con List.FilterByBoolMask para devolver una lista sin valores nulos.
___
## Archivo de ejemplo

![IsNull](./DSCore.Object.IsNull_img.jpg)

