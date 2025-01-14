## En detalle:
Fillet devolverá un sólido nuevo con bordes redondeados. La entrada de bordes especifica los bordes que se van a empalmar, mientras que la entrada de desfase determina el radio del empalme. En el siguiente ejemplo, se utiliza primero un cubo con las entradas por defecto. Para obtener los bordes adecuados del cubo, se descompone primero este a fin de obtener las caras como una lista de superficies. A continuación, se usa un nodo Face.Edges para extraer los bordes del cubo. Se extrae el primer borde de cada cara con GetItemAtIndex. Un control deslizante de número ajusta el radio de cada empalme.
___
## Archivo de ejemplo



