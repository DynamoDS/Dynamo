## Description approfondie
`List.UniqueItems` supprime tous les éléments en double dans une liste d'entrée en créant une nouvelle liste qui comprend uniquement les éléments qui apparaissent une seule fois dans la liste d'origine.

Dans l'exemple ci-dessous, nous utilisons `Math.RandomList` pour générer en premier lieu une liste de nombres aléatoires compris entre 0 et 1. Nous multiplions ensuite par 10 et nous utilisons une opération `Math.Floor` pour renvoyer une liste de nombres entiers aléatoires compris entre 0 et 9, dont beaucoup sont répétés plusieurs fois. Nous utilisons `List.UniqueItems` pour créer une liste dans laquelle chaque nombre entier apparait une seule fois. L'ordre de la liste de sortie est basé sur la première occurrence trouvée pour un élément.
___
## Exemple de fichier

![List.UniqueItems](./DSCore.List.UniqueItems_img.jpg)
