## Description approfondie
`List.Transpose` permute les lignes et les colonnes d'une liste de listes. Par exemple, une liste contenant 5 sous-listes de 10 éléments chacune serait transposée en 10 listes de 5 éléments chacune. Des valeurs nulles sont insérées au besoin pour garantir que chaque sous-liste comporte le même nombre d'éléments.

Dans l'exemple, nous générons une liste de nombres de 0 à 5 et une autre liste de lettres de A à E. Nous utilisons ensuite le noeud `List.Create` pour les combiner. `List.Transpose` génère 6 listes de 2 éléments chacune, avec un nombre et une lettre par liste. Notez que, étant donné que l'une des listes d'origine était plus longue que l'autre, `List.Transpose` a inséré une valeur nulle pour l'élément non apparié.
___
## Exemple de fichier

![List.Transpose](./DSCore.List.Transpose_img.jpg)
