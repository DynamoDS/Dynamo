## Détails
'Mesh.Cone' crée un cône de maillage dont la base est centrée sur un point d'origine d'entrée, avec une valeur d'entrée pour les rayons de base et supérieurs, la hauteur et un certain nombre de 'divisions'. Le nombre de divisions correspond au nombre de sommets créés en haut et en bas du cône. Si le nombre de divisions est égal à 0, Dynamo utilise une valeur par défaut. Le nombre de divisions le long de l'axe Z est toujours égal à 5. L'entrée 'cap' utilise un booléen pour contrôler si le cône est fermé en haut.
Dans l'exemple ci-dessous, le noeud 'Mesh.Cone' est utilisé pour créer un maillage en forme de cône avec 6 divisions, ainsi la base et le sommet du cône sont des hexagones. Le noeud 'Mesh.Triangles' est utilisé pour visualiser la distribution des triangles de maillage.


## Exemple de fichier

![Example](./Autodesk.DesignScript.Geometry.Mesh.Cone_img.jpg)
