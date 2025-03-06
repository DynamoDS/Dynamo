## Description approfondie
`Geometry.ImportFromSATWithUnits` importe une géométrie dans Dynamo à partir d'un fichier .SAT et d'un noeud `DynamoUnit.Unit` qui est convertible en millimètres. Ce noeud prend un objet ou un chemin de fichier comme première entrée et une valeur `dynamoUnit` comme deuxième entrée. Si l'entrée `dynamoUnit` est nulle, la géométrie .SAT est importée sans unité, en important simplement les données du fichier sans conversion d'unité. Si une unité est indiquée, les unités internes du fichier .SAT sont converties dans les unités spécifiées.

Dynamo est sans unité, mais les valeurs numériques de votre graphique Dynamo ont probablement toujours une unité implicite. Vous pouvez utiliser l'entrée `dynamoUnit` pour mettre à l'échelle la géométrie interne du fichier .SAT vers ce système d'unités.

Dans l'exemple ci-dessous, la géométrie est importée à partir d'un fichier .SAT, en utilisant les pieds comme unité. Pour que ce fichier d'exemple fonctionne sur votre ordinateur, téléchargez ce fichier d'exemple .SAT et pointez le noeud `File Path` vers le fichier invalid.sat.

___
## Exemple de fichier

![Geometry.ImportFromSATWithUnits](./GeometryUI.ImportFromSATWithUnits_img.jpg)
