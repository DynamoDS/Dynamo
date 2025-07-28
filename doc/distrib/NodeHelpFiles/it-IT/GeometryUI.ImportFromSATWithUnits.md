## In profondità
`Geometry.ImportFromSATWithUnits` importa la geometria in Dynamo da un file SAT e `DynamoUnit.Unit` convertibile dai millimetri. Questo nodo utilizza un oggetto file o un percorso di file come primo input e `dynamoUnit` come secondo. Se l'input `dynamoUnit` viene lasciato null, la geometria del file SAT viene importata senza unità, importando semplicemente i dati geometrici nel file senza alcuna conversione di unità. Se viene trasferita un'unità, le unità interne del file SAT vengono convertite nelle unità specificate.

Dynamo è senza unità, ma i valori numerici nel grafico di Dynamo probabilmente presentano ancora alcune unità implicite. È possibile utilizzare l'input `dynamoUnit` per adattare in scala la geometria interna del file SAT in tale sistema di unità.

Nell'esempio seguente, la geometria viene importata da un file SAT, con l'unità piedi. Per far funzionare questo file di esempio nel computer, scaricare questo file SAT di esempio e puntare il nodo `File Path` al file .sat non valido.

___
## File di esempio

![Geometry.ImportFromSATWithUnits](./GeometryUI.ImportFromSATWithUnits_img.jpg)
