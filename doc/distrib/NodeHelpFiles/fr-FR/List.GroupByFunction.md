## Description approfondie
`List.GroupByFunction` renvoie une nouvelle liste regroupée selon une fonction.

L'entrée `groupFunction` nécessite un noeud dans un état de fonction (c'est-à-dire qui renvoie une fonction). Cela signifie qu'au moins une des entrées du noeud n'est pas connectée. Dynamo exécute ensuite la fonction de noeud sur chaque élément de la liste d'entrée du noeud `List.GroupByFunction` afin d'utiliser la sortie comme mécanisme de regroupement.

Dans l'exemple ci-dessous, deux listes différentes sont regroupées à l'aide du noeud `List.GetItemAtIndex` comme fonction. Cette fonction crée des groupes (une nouvelle liste) à partir de chaque index de niveau supérieur.
___
## Exemple de fichier

![List.GroupByFunction](./List.GroupByFunction_img.jpg)
