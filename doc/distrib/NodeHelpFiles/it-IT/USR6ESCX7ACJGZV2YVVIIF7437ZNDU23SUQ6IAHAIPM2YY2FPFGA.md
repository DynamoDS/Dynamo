## In profondità
`TSplineSurface.Thicken(vector, softEdges)` ispessisce una superficie T-Spline guidata dal vettore specificato. L'operazione di ispessimento duplica la superficie nella direzione `vector` e quindi collega le due superfici unendo i loro bordi. L'input booleano `softEdges` controlla se i bordi risultanti sono lisciati (true) o triangolati (false).

Nell'esempio seguente, viene ispessita una superficie estrusa T-Spline utilizzando il nodo `TSplineSurface.Thicken(vector, softEdges)`. La superficie risultante viene traslata sul lato per una migliore visualizzazione.


___
## File di esempio

![TSplineSurface.Thicken](./USR6ESCX7ACJGZV2YVVIIF7437ZNDU23SUQ6IAHAIPM2YY2FPFGA_img.jpg)
