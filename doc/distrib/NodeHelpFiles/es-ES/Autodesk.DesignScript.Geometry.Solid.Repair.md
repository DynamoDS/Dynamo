## En detalle:
`Solid.Repair` intenta reparar los sólidos que tienen una geometría no válida y realizar posibles optimizaciones. `Solid.Repair` devolverá un nuevo objeto sólido.

Este nodo es útil cuando se producen errores al realizar operaciones en geometría importada o convertida.

En el ejemplo siguiente, `Solid.Repair` se utiliza para reparar la geometría de un archivo **.SAT**. La geometría del archivo no puede realizar operaciones de conversión a booleano o recorte y `Solid.Repair` limpia cualquier *geometría no válida* que esté provocando el error.

Por lo general, esta funcionalidad no debería ser necesaria en geometrías creadas por usted en Dynamo, solo en geometrías procedentes de fuentes externas. Si descubre que esto no es así, informe de un error al equipo de Dynamo Github.
___
## Archivo de ejemplo

![Solid.Repair](./Autodesk.DesignScript.Geometry.Solid.Repair_img.jpg)
