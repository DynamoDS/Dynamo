## Description approfondie
`List.TakeEveryNthItem` produit une nouvelle liste contenant uniquement les éléments de la liste d'entrée qui se trouvent à des intervalles de la valeur n d'entrée. Le point de départ de l'intervalle peut être modifié dans l'entrée `offset`. Par exemple, si vous entrez 3 dans n et que vous laissez la valeur `offset` par défaut de 0, les éléments conserveront les index 2, 5, 8, etc. Avec une valeur `offset` de 1, les éléments auront les index 0, 3, 6, etc. Notez que le décalage tient compte de la liste entière. Pour supprimer les éléments sélectionnés au lieu de les conserver, reportez-vous au noeud `List.DropEveryNthItem`.

Dans l'exemple ci-dessous, nous générons d'abord une liste de nombres en utilisant `Range`, puis nous conservons un nombre sur deux en utilisant 2 comme entrée pour n.
___
## Exemple de fichier

![List.TakeEveryNthItem](./DSCore.List.TakeEveryNthItem_img.jpg)
