## In-Depth
`TSplineEdge.Info` restituisce le seguenti proprietà del bordo di una superficie T-Spline:
- `uvnFrame`: punto sullo scafo, vettore U, vettore V e vettore normale del bordo T-Spline
- `index`: indice del bordo
- `isBorder`: indica se quello scelto è un bordo della superficie T-Spline
- `isManifold`: indica se il bordo scelto è manifold

Nell'esempio seguente, `TSplineTopology.DecomposedEdges`viene utilizzato per ottenere un elenco di tutti i bordi di una superficie della primitiva del cilindro T-Spline e `TSplineEdge.Info` viene utilizzato per esaminare le relative proprietà.


## File di esempio

![Example](./Autodesk.DesignScript.Geometry.TSpline.TSplineEdge.Info_img.jpg)
