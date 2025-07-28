## Description approfondie
Geometry ImportFromSAT importe la géométrie dans Dynamo à partir d'un type de fichier SAT. Ce nœud prend un filePath comme entrée et accepte également une chaîne avec un chemin de fichier valide. Dans l'exemple ci-dessous, nous avons précédemment exporté la géométrie vers un fichier SAT (voir ExportToSAT). Le nom de fichier que nous avons choisi était example.sat et il a été exporté vers un dossier sur le bureau de l'utilisateur. Dans cet exemple, deux nœuds différents sont utilisés pour importer la géométrie à partir d'un fichier SAT. L'un possède le type d'entrée filePath et l'autre possède le type d'entrée "file". Le type d'entrée filePath est créé à l'aide d'un nœud FilePath, qui permet de sélectionner un fichier en cliquant sur le bouton Parcourir. Dans le deuxième exemple, nous spécifions le chemin d'accès au fichier manuellement à l'aide d'un élément de chaîne.
___
## Exemple de fichier

![ImportFromSAT (filePath)](./Autodesk.DesignScript.Geometry.Geometry.ImportFromSAT(filePath)_img.jpg)

