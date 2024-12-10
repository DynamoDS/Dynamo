## En detalle:
En el ejemplo siguiente, se crea un cuadro de T-Spline mediante el nodo `TSplineSurface.ByBoxLengths` con un origen, una anchura, una longitud, una altura, tramos y una simetría especificados.
A continuación, se utiliza `EdgeByIndex` para seleccionar una arista de la lista de aristas de la superficie generada. A continuación, se desliza la arista seleccionada a lo largo de las aristas adyacentes mediante `TSplineSurface.SlideEdges`, seguida de sus equivalentes simétricos.
___
## Archivo de ejemplo

![TSplineTopology.EdgeByIndex](./Autodesk.DesignScript.Geometry.TSpline.TSplineTopology.EdgeByIndex_img.jpg)
