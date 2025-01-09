## Description approfondie
`List.AllFalse` renvoie la valeur False si un élément de la liste donnée a la valeur True ou n'est pas une valeur booléenne. `List.AllFalse` renvoie la valeur True uniquement si tous les éléments de la liste indiquée sont des valeurs booléennes et False.

Dans l'exemple ci-dessous, nous utilisons `List.AllFalse` pour évaluer les listes de valeurs booléennes. La première liste possède une valeur True, donc la valeur False est renvoyée. La deuxième liste présente uniquement des valeurs False, donc la valeur True est renvoyée. La troisième liste comprend une sous-liste qui inclut une valeur True, donc la valeur False est renvoyée. Le noeud final évalue les deux sous-listes et renvoie False pour la première sous-liste, car elle présente uniquement des valeurs False.
___
## Exemple de fichier

![List.AllFalse](./DSCore.List.AllFalse_img.jpg)
