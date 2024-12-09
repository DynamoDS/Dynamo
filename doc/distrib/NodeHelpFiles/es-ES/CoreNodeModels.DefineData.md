## En detalle:

El nodo "Define Data" valida el tipo de los datos entrantes. Puede utilizarse para garantizar que los datos locales sean del tipo deseado y también se ha diseñado para su uso como nodo de entrada o salida que indica el tipo de datos que un gráfico espera o proporciona. El nodo admite una selección de tipos de datos de Dynamo de uso común, por ejemplo, "String", "Point" o "Boolean". La lista completa de tipos de datos admitidos se encuentra en el menú desplegable del nodo. El nodo admite datos en forma de valor único o de lista plana. No admite listas anidadas, diccionarios ni replicación.

### Comportamiento

El nodo valida los datos procedentes del puerto de entrada en función de la configuración del menú desplegable Tipo y del conmutador Lista (consulte los detalles que aparecen a continuación). Si la validación se realiza correctamente, la salida del nodo es la misma que la entrada. Si no se realiza correctamente, el nodo pasará al estado de advertencia con una salida nula.
El nodo presenta una entrada:

- La entrada "**>**": conecta con un nodo ascendente para validar el tipo de datos.
    Además, el nodo ofrece tres controles de usuario:
- El conmutador **Detección automática de tipo**: si está activado, el nodo analiza los datos entrantes y, si los datos son de un tipo admitido, el nodo establece los valores de los controles de Tipo y Lista en función del tipo de datos entrantes. El menú desplegable Tipo y el conmutador Lista están desactivados y se actualizarán automáticamente en función del nodo de entrada.

    Si la opción Detección automática de tipo está desactivada, puede especificar un tipo de datos mediante el menú Tipo y la opción Lista. Si los datos entrantes no coinciden con lo que ha especificado, el nodo pasará al estado de advertencia con una salida nula.
- El menú desplegable **Tipo**: establece el tipo de datos esperado. Si el control está activado (el conmutador **Detección automática de tipo** está desactivado), establezca un tipo de datos para la validación. Si el control está desactivado (el conmutador **Detección automática de tipo** está activado), el tipo de datos se establece automáticamente en función de los datos entrantes. Los datos son válidos si su tipo coincide exactamente con el tipo mostrado o si su tipo es un elemento secundario del tipo mostrado (por ejemplo, si el menú desplegable Tipo se establece en "Curva", son válidos los objetos de tipo "Rectángulo", "Línea", etc.).
- El conmutador **Lista**: si está activado, el nodo espera que los datos entrantes sean una única lista plana que contenga elementos de un tipo de datos válido (consulte la información anterior). Si está desactivado, el nodo espera un único elemento de un tipo de datos válido.

### Utilizar como nodo de entrada

Cuando se establece como entrada ("Es entrada" en el menú contextual del nodo), el nodo puede utilizar de forma opcional nodos ascendentes para establecer el valor por defecto de la entrada. Una ejecución del gráfico almacenará en caché el valor del nodo "Define data" para utilizarlo cuando se ejecute el gráfico externamente, por ejemplo, con el nodo "Engine".

## Archivo de ejemplo

En el ejemplo siguiente, el primer grupo de nodos "DefineData" tiene la opción **Detección automática de tipo** desactivada. El nodo valida correctamente la entrada "Number" proporcionada al rechazar la entrada "String". El segundo grupo contiene un nodo con la opción **Detección automática de tipo** activada. El nodo ajusta automáticamente el menú desplegable Tipo y el conmutador Lista para que coincidan con la entrada, en este caso, una lista de enteros.

![Define_Data](./CoreNodeModels.DefineData_img.png)
