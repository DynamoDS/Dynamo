## Description approfondie
`List.Chop` divise une liste donnée en plusieurs listes en fonction des longueur de nombre entier saisies en entrée. La première liste imbriquée contient le nombre d'éléments spécifié par le premier nombre dans l'entrée `lengths`. La deuxième liste imbriquée contient le nombre d'éléments spécifié par le deuxième nombre dans l'entrée `lengths`, etc. `List.Chop` répète le dernier nombre dans l'entrée `lengths` jusqu'à ce que tous les éléments de la liste d'entrée soient divisés.

Dans l'exemple ci-dessous, nous utilisons un Code Block pour générer une plage de nombres entre 0 et 5, avec incréments de 1. Cette liste contient 6 éléments. Nous utilisons un deuxième Code Block pour créer une liste de longueurs selon lesquelles diviser la première liste. Le premier nombre de cette liste est 1, que `List.Chop` utilise pour créer une liste imbriquée avec 1 élément. Le deuxième numéro est 3, ce qui crée une liste imbriquée avec 3 éléments. Puisqu'il s'agit des seules longueurs indiquées, `List.Chop` inclut tous les éléments restants dans la troisième et dernière liste imbriquée.
___
## Exemple de fichier

![List.Chop](./DSCore.List.Chop_img.jpg)
