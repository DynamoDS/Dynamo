## Description approfondie
`List.AllTrue` renvoie la valeur False si un élément de la liste donnée est une valeur False ou non booléenne. `List.AllTrue` renvoie la valeur True uniquement si tous les éléments de la liste indiquée sont des valeurs booléennes et True.

Dans l'exemple ci-dessous, nous utilisons `List.AllTrue` pour évaluer les listes de valeurs booléennes. La première liste possède une valeur False, la valeur False est donc renvoyée. La deuxième liste présente uniquement des valeurs True, donc la valeur True est renvoyée. La troisième liste comprend une sous-liste qui inclut une valeur False, donc la valeur False est renvoyée. Le noeud final évalue les deux sous-listes et renvoie False pour la première car elle contient une valeur False et True pour la deuxième car elle comprend uniquement des valeurs True.
___
## Exemple de fichier

![List.AllTrue](./DSCore.List.AllTrue_img.jpg)
