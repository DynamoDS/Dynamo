## En detalle
Este nodo devuelve una nueva malla suavizada mediante un algoritmo de suavizado cotangente que no separa los vértices de su posición original y es mejor para conservar las características y las aristas. Es necesario introducir un valor de escala en el nodo para establecer la escala espacial del suavizado. Los valores de escala pueden oscilar entre 0,1 y 64,0. Los valores más altos generan un efecto de suavizado más perceptible, dando como resultado lo que parece ser una malla más simple. A pesar de parecer más suave y sencilla, la nueva malla tiene el mismo número de triángulos, aristas y vértices que la inicial.

En el ejemplo siguiente, se utiliza `Mesh.ImportFile` para importar un objeto. A continuación, se utiliza `Mesh.Smooth` para suavizar el objeto con una escala de suavizado de cinco. A continuación, se traslada el objeto a otra posición con `Mesh.Translate` para obtener una mejor vista preliminar y se utiliza `Mesh.TriangleCount` para realizar un seguimiento del número de triángulos en la malla antigua y en la nueva.

## Archivo de ejemplo

![Example](./Autodesk.DesignScript.Geometry.Mesh.Smooth_img.jpg)
