## En detalle

La opción de gráfico de líneas XY crea un gráfico con una o varias líneas trazadas por sus valores X e Y. Etiquete las líneas o cambie el número de líneas mediante la introducción de una lista de etiquetas de cadena en la entrada de etiquetas. Cada etiqueta crea una nueva línea codificada por colores. Si solo introduce un valor de cadena, solo se creará una línea.

Para determinar la colocación de cada punto a lo largo de cada línea, utilice una lista de listas que contengan valores dobles para las entradas de valores X e Y. Debe haber un número igual de valores en las entradas de valores X e Y. El número de sublistas también debe coincidir con el número de valores de cadena en la entrada de etiquetas.
Por ejemplo, si desea crear 3 líneas, cada una con 5 puntos, especifique una lista con 3 valores de cadena en la entrada de etiquetas para asignar un nombre a cada línea y proporcione 3 sublistas con 5 valores dobles en cada una para los valores X e Y.

Para asignar un color a cada línea, introduzca una lista de colores en la entrada de colores. Al asignar colores personalizados, el número de colores debe coincidir con el número de valores de cadena en la entrada de etiquetas. Si no se han asignado colores, se utilizarán colores aleatorios.

___
## Archivo de ejemplo

![XY Line Plot](./CoreNodeModelsWpf.Charts.XYLineChartNodeModel_img.jpg)

