## In-Depth
`TSplineFace.Index` restituisce l'indice della faccia sulla superficie T-Spline. Tenere presente che nella topologia di una superficie T-Spline, gli indici di Face, Edge e Vertex non coincidono necessariamente con il numero di sequenza dell'elemento nell'elenco. Per risolvere il problema, utilizzare il nodo `TSplineSurface.CompressIndices`.

Nell'esempio seguente, `TSplineFace.Index` viene utilizzato per mostrare gli indici di tutte le facce regolari di una superficie T-Spline.

## File di esempio

![Example](./Autodesk.DesignScript.Geometry.TSpline.TSplineFace.Index_img.jpg)
