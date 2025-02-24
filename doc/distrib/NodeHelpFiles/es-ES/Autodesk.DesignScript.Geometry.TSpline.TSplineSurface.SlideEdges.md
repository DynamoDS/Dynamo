## En detalle:
En el ejemplo siguiente, se crea una superficie de cuadro de T-Spline sencilla y se selecciona una de sus aristas mediante el nodo `TSplineTopology.EdgeByIndex`. Para comprender mejor la posición del vértice elegido, este se visualiza con la ayuda de los nodos `TSplineEdge.UVNFrame` y `TSplineUVNFrame.Position`. La arista elegida se transfiere como entrada del nodo `TSplineSurface.SlideEdges`, junto con la superficie a la que pertenece. La entrada `amount` determina la distancia que se desplaza la arista hacia sus aristas adyacentes, expresada en forma de porcentaje. La entrada `roundness` controla la planicidad o la redondez del bisel. El efecto de la redondez se entiende mejor en el modo de cuadro. El resultado de la operación de deslizamiento se traslada a un lado para su previsualización.

___
## Archivo de ejemplo

![TSplineSurface.SlideEdges](./Autodesk.DesignScript.Geometry.TSpline.TSplineSurface.SlideEdges_img.jpg)
