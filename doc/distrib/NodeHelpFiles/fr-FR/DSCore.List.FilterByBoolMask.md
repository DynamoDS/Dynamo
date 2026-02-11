## Description approfondie
`List.FilterByBoolMask` prend deux listes comme entrées. La première liste est divisée en deux listes distinctes selon une liste correspondante de valeurs booléennes (True ou False). Les éléments de l'entrée `list` qui correspondent à True dans l'entrée `mask` sont indiqués dans la sortie `In`, tandis que les éléments qui correspondent à une valeur False sont indiqués dans la sortie `out`.

Dans l'exemple ci-dessous, `List.FilterByBoolMask` est utilisé pour extraire le bois et le stratifié dans une liste de matériaux. Nous comparons d'abord deux listes pour trouver des éléments correspondants, puis nous utilisons un opérateur `Or` pour vérifier les éléments de la liste True. Ensuite, les éléments de la liste sont filtrés selon le matériau bois, stratifié, ou autre.
___
## Exemple de fichier

![List.FilterByBoolMask](./DSCore.List.FilterByBoolMask_img.jpg)
