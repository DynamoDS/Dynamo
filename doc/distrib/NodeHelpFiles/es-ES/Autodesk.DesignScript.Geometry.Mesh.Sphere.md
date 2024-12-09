## En detalle
`Mesh.Sphere` crea una malla esférica, centrada en el punto de origen introducido, con un radio y un número de divisiones especificados. La entrada booleana `icosphere` se utiliza para alternar entre los tipos de malla esférica `icosphere` y `UV-Sphere`. Una malla icosfera cubre la esfera con triángulos más regulares que una malla UV y suele dar mejores resultados en las operaciones de modelado posteriores. En una malla UV, los polos están alineados con el eje de la esfera y las capas de triángulos se generan longitudinalmente alrededor de este eje.

En el caso de la icosfera, el número de triángulos alrededor del eje de la esfera podría ser tan bajo como el número especificado de divisiones y como máximo el doble de ese número. Las divisiones de `UV-sphere` determinan el número de capas de triángulos generadas longitudinalmente alrededor de la esfera. Cuando la entrada `divisions` se establece en cero, el nodo devuelve una esfera UV con un número por defecto de 32 divisiones para cualquier tipo de malla.

En el ejemplo siguiente, el nodo `Mesh.Sphere` se utiliza para crear dos esferas con radios y divisiones idénticos, pero con métodos diferentes. Cuando la entrada `icosphere` se establece en `True`, el nodo `Mesh.Sphere` devuelve una `icosphere`. Por el contrario, si el valor de la entrada `icosphere` es `False`, el nodo `Mesh.Sphere` devuelve una `UV-sphere`.

## Archivo de ejemplo

![Example](./Autodesk.DesignScript.Geometry.Mesh.Sphere_img.jpg)
