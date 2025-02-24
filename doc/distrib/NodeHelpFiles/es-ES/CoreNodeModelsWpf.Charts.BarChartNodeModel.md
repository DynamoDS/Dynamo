## En detalle

El gráfico de barras crea un gráfico con barras orientadas verticalmente. Las barras pueden organizarse en varios grupos y etiquetarse mediante la codificación por colores. Tiene la opción de crear un único grupo mediante la introducción de un único valor doble o varios grupos mediante la introducción de varios valores dobles por sublista en la entrada de valores. Para definir categorías, inserte una lista de valores de cadena en la entrada de etiquetas. Cada valor crea una nueva categoría codificada por colores.

Para asignar un valor (altura) a cada barra, introduzca una lista de listas que contengan valores dobles en la entrada de valores. Cada sublista determinará el número de barras y a qué categoría pertenecen, en el mismo orden que la entrada de etiquetas. Si tiene una única lista de valores dobles, solo se creará una categoría. El número de valores de cadena en la entrada de etiquetas debe coincidir con el número de sublistas en la entrada de valores.

Para asignar un color a cada categoría, introduzca una lista de colores en la entrada de colores. Al asignar colores personalizados, el número de colores debe coincidir con el número de valores de cadena en la entrada de etiquetas. Si no se han asignado colores, se utilizarán colores aleatorios.

## Ejemplo: grupo único

Imagine que desea representar las valoraciones medias de los usuarios de un elemento durante los tres primeros meses del año. Para visualizar esto, necesita una lista de tres valores de cadena, etiquetados como enero, febrero y marzo.
Por lo tanto, para la entrada de etiquetas, proporcionaremos la siguiente lista en un bloque de código:

[“Enero”, “Febrero”, “Marzo”];

También puede utilizar nodos de cadena conectados al nodo de creación de listas para crear la lista.

A continuación, en la entrada de valores, introduciremos la valoración media de los usuarios para cada uno de los tres meses como una lista de listas:

[[3.5], [5], [4]];

Tenga en cuenta que, como tenemos tres etiquetas, necesitamos tres sublistas.

Ahora, cuando se ejecute el gráfico, se creará el diagrama de barras, en el que cada barra coloreada representará la valoración media de los clientes durante el mes. Puede seguir utilizando los colores por defecto o introducir una lista de colores personalizados en la entrada de colores.

## Ejemplo: varios grupos

Puede aprovechar la función de agrupación del nodo de gráfico de barras mediante la introducción de más valores en cada sublista en la entrada de valores. En este ejemplo, vamos a crear un gráfico que muestre el número de puertas en tres variaciones de tres modelos, Modelo A, Modelo B y Modelo C.

Para ello, proporcionaremos primero las etiquetas:

[“Modelo A”, “Modelo B”, “Modelo C”];

A continuación, proporcionaremos valores, asegurándonos una vez más de que el número de sublistas coincida con el número de etiquetas:

[[17, 9, 13],[12,11,15],[15,8,17]];

Ahora, al hacer clic en Ejecutar, el nodo de gráfico de barras creará un gráfico con tres grupos de barras, marcados con los índices 0, 1 y 2, respectivamente. En este ejemplo, considere cada índice (es decir, grupo) una variación de diseño. Los valores del primer grupo (Índice 0) se extraen del primer elemento de cada lista de la entrada de valores, por lo que el primer grupo contiene 17 para el modelo A, 12 para el Modelo B y 15 para el Modelo C. El segundo grupo (Índice 1) utiliza el segundo valor de cada grupo, y así sucesivamente.

___
## Archivo de ejemplo

![Bar Chart](./CoreNodeModelsWpf.Charts.BarChartNodeModel_img.jpg)

