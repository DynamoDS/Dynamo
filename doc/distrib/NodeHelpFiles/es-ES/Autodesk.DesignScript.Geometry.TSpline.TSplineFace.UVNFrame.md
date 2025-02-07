## In-Depth
Un marco UVNF de una cara proporciona información útil sobre la posición y la orientación de la cara al devolver el vector normal y las direcciones UV.
En el ejemplo siguiente, se utiliza un nodo `TSplineFace.UVNFrame` para visualizar la distribución de caras en una primitiva de esfera de malla cuadrada. `TSplineTopology.DecomposedFaces` se utiliza para consultar todas las caras y, a continuación, se usa un nodo `TSplineFace.UVNFrame` para recuperar las posiciones de los centroides de cara como puntos. Los puntos se visualizan mediante un nodo `TSplineUVNFrame.Position`. Las etiquetas se muestran en la vista preliminar en segundo plano mediante la activación de Mostrar etiquetas en el menú contextual del nodo.

## Archivo de ejemplo

![Example](./Autodesk.DesignScript.Geometry.TSpline.TSplineFace.UVNFrame_img.jpg)
