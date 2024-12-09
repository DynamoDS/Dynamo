## Description approfondie
`List.NormalizeDepth` renvoie une nouvelle liste de profondeurs uniformes à un rang ou une profondeur de liste spécifiés.

Comme pour `List.Flatten`, vous pouvez utiliser le noeud `List.NormalizeDepth` pour renvoyer une liste unidimensionnelle (liste avec un seul niveau). Mais vous pouvez également l'utiliser pour ajouter des niveaux de liste. Le noeud normalise la liste d'entrée selon la profondeur de votre choix.

Dans l'exemple ci-dessous, une liste contenant deux listes de profondeur inégale peut être normalisée à différents rangs à l'aide d'un curseur de nombres entiers. En normalisant les profondeurs à différents rangs, la profondeur de la liste augmente ou diminue, mais reste uniforme. Une liste de rang 1 renvoie une seule liste d'éléments, tandis qu'une liste de rang 3 renvoie deux niveaux de sous-listes.
___
## Exemple de fichier

![List.NormalizeDepth](./DSCore.List.NormalizeDepth_img.jpg)
