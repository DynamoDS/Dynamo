## In profondità
`Geometry.DeserializeFromSABWithUnits` importa la geometria in Dynamo da una matrice di byte SAB (Standard ACIS Binary) e `DynamoUnit.Unit` convertibile da millimetri. Questo nodo utilizza byte[] come primo input e `dynamoUnit` come secondo. Se l'input `dynamoUnit` è lasciato null, questo importa la geometria del file SAB senza unità, importando i dati geometrici nella matrice senza alcuna conversione di unità. Se viene fornita un'unità, le unità interne della matrice SAB vengono convertite nelle unità specificate.

Dynamo è senza unità, ma i valori numerici nel grafico di Dynamo probabilmente presentano ancora alcune unità implicite. È possibile utilizzare l'input `dynamoUnit` per adattare in scala la geometria interna del file .SAB in tale sistema di unità.

Nell'esempio seguente, viene generato un cuboide da file in formato SAB con 2 unità di misura (senza unità). L'input `dynamoUnit` mette in scala l'unità scelta per l'utilizzo in altro software.

___
## File di esempio

![Geometry.DeserializeFromSABWithUnits](./GeometryUI.DeserializeFromSABWithUnits_img.jpg)
