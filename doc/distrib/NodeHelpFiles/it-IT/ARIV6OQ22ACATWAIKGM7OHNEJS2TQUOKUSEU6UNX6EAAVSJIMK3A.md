<!--- Autodesk.DesignScript.Geometry.TSpline.TSplineSurface.CompressIndexes --->
<!--- ARIV6OQ22ACATWAIKGM7OHNEJS2TQUOKUSEU6UNX6EAAVSJIMK3A --->
## In profondità
Il nodo `TSplineSurface.CompressIndexes` rimuove eventuali spazi nei numeri di indici di bordi, vertici o facce di una superficie T-Spline che derivano da varie operazioni, ad esempio Elimina faccia. L'ordine degli indici viene mantenuto.

Nell'esempio seguente, viene eliminato un numero di facce da una superficie della primitiva quadball che influisce sugli indici dei bordi della forma. `TSplineSurface.CompressIndexes` viene utilizzato per correggere gli indici dei bordi della forma e quindi diventa possibile selezionare un bordo con l'indice 1.

## File di esempio

![Example](./ARIV6OQ22ACATWAIKGM7OHNEJS2TQUOKUSEU6UNX6EAAVSJIMK3A_img.jpg)
