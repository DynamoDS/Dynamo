## Description approfondie
ReplaceByCondition prend une liste donnée et évalue chaque élément avec une condition donnée. Si la condition est évaluée sur "True", l'élément correspondant est remplacé dans la liste de sortie par l'élément spécifié dans l'entrée replaceWith. Dans l'exemple ci-dessous, nous utilisons un nœud Formula et entrons la formule x%2==0, qui recherche le reste d'un élément donné après la division par 2, puis vérifie si ce reste est égal à 0. Cette formule renvoie "True" pour les nombres entiers pairs. Notez que l'entrée x est vide. L'utilisation de cette formule comme condition dans un nœud ReplaceByCondition entraîne la création d'une liste de sortie dans laquelle chaque nombre pair est remplacé par l'élément spécifié, dans ce cas le nombre entier 10.
___
## Exemple de fichier

![ReplaceByCondition](./CoreNodeModels.HigherOrder.Replace_img.jpg)

