## En detalle:
Chamfer devolverá un nuevo sólido con bordes achaflanados. La entrada de bordes especifica los bordes que se van a achaflanar, mientras que la entrada de desfase determina la extensión del chaflán. En el siguiente ejemplo, se comienza por un cubo con las entradas por defecto. Para obtener los bordes adecuados del cubo, se descompone primero este a fin de obtener las caras como una lista de superficies. A continuación, se utiliza un nodo Face.Edges para extraer los bordes del cubo. Se extrae el primer borde de cada cara con GetItemAtIndex. Un control deslizante de número permite ajustar la distancia de desfase del chaflán.
___
## Archivo de ejemplo

![Chamfer](./Autodesk.DesignScript.Geometry.Solid.Chamfer_img.jpg)

