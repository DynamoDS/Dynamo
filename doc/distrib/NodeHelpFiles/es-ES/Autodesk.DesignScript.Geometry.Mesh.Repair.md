## En detalle
Devuelve una malla nueva con los siguientes defectos reparados:
- Componentes pequeños: si la malla contiene segmentos desconectados muy pequeños (en relación con el tamaño total de la malla), se descartarán.
- Agujeros: se rellenan los agujeros de la malla.
- Regiones no múltiples: si un vértice está conectado a más de dos aristas de *contorno* o una arista está conectada a más de dos triángulos, ese vértice o arista no es múltiple. El kit de herramientas de malla eliminará la geometría hasta que la malla sea múltiple.
Este método intenta conservar la mayor cantidad posible de la malla original, a diferencia de `MakeWatertight`, que vuelve a muestrear la malla.

En el ejemplo siguiente, se utiliza `Mesh.Repair` en una malla importada para rellenar el agujero alrededor de la oreja del conejito.

## Archivo de ejemplo

![Example](./Autodesk.DesignScript.Geometry.Mesh.Repair_img.jpg)
