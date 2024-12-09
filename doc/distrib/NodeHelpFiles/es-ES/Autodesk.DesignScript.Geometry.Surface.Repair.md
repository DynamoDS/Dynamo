## En detalle:
Repair intentará reparar superficies o PolySurfaces, que presentan geometría no válida, así como realizar optimizaciones potenciales. Este nodo devolverá un nuevo objeto de superficie.
Este nodo es útil cuando se producen errores al realizar operaciones en geometría importada o convertida.

Por ejemplo, si importa datos de un contexto anfitrión como **Revit** o desde un archivo **.SAT** y detecta que presenta inesperadamente un error al establecer como booleano o recortar, es posible que una operación de reparación limpie la *geometría no válida* que está provocando el error.

Por lo general, no es necesario utilizar esta función en la geometría creada en Dynamo, solo en la geometría de fuentes externas. Si no es así, informe de un error al equipo de Dynamo.
___


