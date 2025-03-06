## In-Depth
`TSplineVertex.Info` restituisce le seguenti proprietà di un vertice T-Spline:
- `uvnFrame`: punto sullo scafo, vettore U, vettore V e vettore normale del vertice T-Spline
- `index`: indice del vertice scelto sulla superficie T-Spline
- `isStarPoint`: indica se il vertice scelto è un punto a stella
- `isTpoint`: indica se il vertice scelto è un punto a T
- `isManifold`: indica se il vertice scelto è manifold
- `valence`: numero di bordi sul vertice T-Spline scelto
- `functionalValence`:la valenza funzionale di un vertice. Per ulteriori informazioni, vedere la documentazione relativa al nodo `TSplineVertex.FunctionalValence`.

Nell'esempio seguente, `TSplineSurface.ByBoxCorners` e `TSplineTopology.VertexByIndex' vengono utilizzati rispettivamente per creare una superficie T-Spline e selezionarne i vertici. `TSplineVertex.Info` viene utilizzato per raccogliere le informazioni sopra riportate su un vertice scelto.

## File di esempio

![Example](./Autodesk.DesignScript.Geometry.TSpline.TSplineVertex.Info_img.jpg)
