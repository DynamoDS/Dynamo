## En detalle:
En el ejemplo siguiente, una superficie de cuadro T-Spline sencilla se transforma en una malla mediante un nodo `TSplineSurface.ToMesh`. La entrada `minSegments` define el número mínimo de segmentos de una cara en cada dirección y es importante para controlar la definición de malla. La entrada `tolerance` corrige imprecisiones mediante la adición de más posiciones de vértice para que coincidan con la superficie original dentro de la tolerancia especificada. El resultado es una malla cuya definición se previsualiza mediante un nodo `Mesh.VertexPositions`.
La malla de salida puede contener triángulos y cuadrantes, lo que es importante tener en cuenta si se utilizan nodos MeshToolkit.
___
## Archivo de ejemplo

![TSplineSurface.ToMesh](./Autodesk.DesignScript.Geometry.TSpline.TSplineSurface.ToMesh_img.jpg)
