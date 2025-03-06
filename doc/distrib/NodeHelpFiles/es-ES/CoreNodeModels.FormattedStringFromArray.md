## En detalle:
Este nodo convertirá un objeto en una cadena. La segunda entrada `format specifier` controla cómo se convierten las entradas numéricas a sus representaciones de cadena.
Esta entrada `format specifier` debe ser uno de los especificadores numéricos de formato estándar de c#.

Los especificadores de formato deben presentar el formato:
`<specifier><precision>`, por ejemplo, F1

Entre los especificadores de formato que se utilizan con frecuencia, se incluyen los siguientes:
```
G : formato general G 1000.0 -> "1000"
F : notación de punto fijo F4 1000.0 -> "1000.0000"
N : número N2 1000 -> "1,000.00"
```

El valor por defecto para este nodo es `G`, que dará como resultado una representación compacta, pero variable.

[Consulte la documentación de Microsoft para obtener información más detallada.](https://learn.microsoft.com/es-es/dotnet/standard/base-types/standard-numeric-format-strings#standard-format-specifiers)
