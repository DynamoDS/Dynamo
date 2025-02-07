## En detalle:
`Geometry.DeserializeFromSABWithUnits` importa geometría a Dynamo desde una matriz de bytes .SAB (Standard ACIS Binary) y `DynamoUnit.Unit` que se puede convertir a partir de milímetros. Este nodo utiliza un "byte[]" como primera entrada y `dynamoUnit` como segunda. Si la entrada `dynamoUnit` es nula, se importa la geometría del archivo .SAB sin unidad, importando los datos geométricos de la matriz sin ninguna conversión de unidades. Si se proporciona una unidad, las unidades internas de la matriz .SAB se convierten a las unidades especificadas.

Dynamo no tiene unidades, pero es probable que los valores numéricos del gráfico de Dynamo sigan teniendo algunas unidades implícitas. Puede utilizar la entrada dynamoUnit para ajustar la escala de la geometría interna del archivo .SAB a ese sistema de unidades.

En el ejemplo siguiente, se genera un ortoedro a partir de SAB con dos unidades de medida (sin unidad). La entrada `dynamoUnit` ajusta la escala de la unidad seleccionada para utilizarla en otro software.

___
## Archivo de ejemplo

![Geometry.DeserializeFromSABWithUnits](./GeometryUI.DeserializeFromSABWithUnits_img.jpg)
