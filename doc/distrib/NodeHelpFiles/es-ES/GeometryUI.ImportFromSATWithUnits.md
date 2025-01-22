## En detalle:
`Geometry.ImportFromSATWithUnits` importa geometría a Dynamo desde un archivo .SAT y `DynamoUnit.Unit`, que se puede convertir a partir de milímetros. Este nodo utiliza un objeto o una ruta de archivo como primera entrada y `dynamoUnit` como segunda. Si la entrada `dynamoUnit` se deja nula, la geometría del archivo .SAT se importa sin unidad, importando simplemente los datos de unidades métricas del archivo sin ninguna conversión de unidades. Si se transfiere una unidad, las unidades internas del archivo .SAT se convierten a las unidades especificadas.

Dynamo no tiene unidades, pero es probable que los valores numéricos del gráfico de Dynamo sigan teniendo algunas unidades implícitas. Puede utilizar la entrada dynamoUnit para ajustar la escala de la geometría interna del archivo .SAT a ese sistema de unidades.

En el ejemplo siguiente, la geometría se importa a partir de un archivo .SAT con pies como unidad. Para que este archivo de ejemplo funcione en el equipo, descargue este archivo SAT de ejemplo y señale el nodo `File Path`' al archivo invalid.sat.

___
## Archivo de ejemplo

![Geometry.ImportFromSATWithUnits](./GeometryUI.ImportFromSATWithUnits_img.jpg)
