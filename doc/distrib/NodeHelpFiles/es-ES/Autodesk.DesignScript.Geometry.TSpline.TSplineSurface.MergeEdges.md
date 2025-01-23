## En detalle:
En el ejemplo siguiente, se crea una superficie de T-Spline mediante la extrusión de una curva NURBS. Seis de sus aristas se seleccionan con un nodo `TSplineTopology.EdgeByIndex`, tres a cada lado de la forma. Los dos conjuntos de aristas, junto con la superficie, se transfieren al nodo `TSplineSurface.MergeEdges`. El orden de los grupos de aristas afecta a la forma; el primer grupo de aristas se desplaza para unirse al segundo grupo, que permanece en el mismo lugar. La entrada `insertCreases` añade la opción de plegar la unión a lo largo de las aristas fusionadas. El resultado de la operación de fusión se traslada a un lado para obtener una mejor vista preliminar.
___
## Archivo de ejemplo

![TSplineSurface.MergeEdges](./Autodesk.DesignScript.Geometry.TSpline.TSplineSurface.MergeEdges_img.gif)
