## In-Depth
Nell'esempio seguente, viene prodotta una superficie non manifold unendo due superfici che condividono un bordo interno. Il risultato è una superficie che non presenta una parte anteriore e una parte posteriore evidenti. La superficie non manifold può essere visualizzata solo in modalità riquadro fino a quando non viene riparata. `TSplineTopology.DecomposedVertices` viene utilizzato per eseguire query su tutti i vertici della superficie e `TSplineVertex.IsManifold` viene utilizzato per evidenziare quali vertici sono considerati manifold. I vertici non manifold vengono estratti e la loro posizione viene visualizzata utilizzando i nodi `TSplineVertex.UVNFrame` e `TSplineUVNFrame.Position`.


## File di esempio

![Example](./Autodesk.DesignScript.Geometry.TSpline.TSplineVertex.IsManifold_img.jpg)
