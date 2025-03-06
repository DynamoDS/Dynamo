## En detalle
`Mesh.ByGeometry` utiliza objetos geométricos de Dynamo ( superficies o sólidos) como entrada y los convierte en una malla. Los puntos y las curvas no tienen representaciones de malla, por lo que no son entradas válidas. La resolución de la malla generada en la conversión está controlada por las dos entradas `tolerance` y `maxGridLines`. La entrada `tolerance` establece la desviación aceptable de la malla respecto a la geometría original y está sujeta al tamaño de la malla. Si el valor de `tolerance` se establece en -1, Dynamo elige una tolerancia razonable. La entrada `maxGridLines` establece el número máximo de líneas de rejilla en la dirección U o V. Un mayor número de líneas de rejilla ayuda a aumentar la suavidad de la triangulación.

## Archivo de ejemplo

![Example](./Autodesk.DesignScript.Geometry.Mesh.ByGeometry_img.jpg)
