## Description approfondie
Les clusters 'List.GroupBySimilarity' établissent la liste des éléments en fonction de la contiguïté de leurs index et de la similitude de leurs valeurs. La liste des éléments à regrouper peut contenir des nombres (entiers et à virgule flottante) ou des chaînes, mais pas un mélange des deux.

Utilisez l'entrée 'tolerance' pour déterminer la similitude des éléments. Pour les listes de nombres, la valeur de 'tolerance' représente la différence maximale admissible entre deux nombres pour qu'ils soient considérés comme similaires.

For string lists, 'tolerance' represents the maximum number of characters that can differ between two strings, using Levenshtein distance for comparison. Maximum tolerance for strings is limited to 10.

L'entrée booléenne 'considerAdjacency' indique si la contiguïté doit être prise en compte lors du regroupement des éléments. Si la valeur est True, seuls les éléments contigus qui sont similaires seront regroupés. Si la valeur est False, seule la similarité sera utilisée pour former des clusters, indépendamment de la contiguïté.

Le noeud renvoie une liste de listes de valeurs regroupées basées sur la contiguïté et la similarité, ainsi qu'une liste de listes des index des éléments regroupés dans la liste d'origine.

Dans l'exemple ci-dessous, 'List.GroupBySimilarity' est utilisé de deux manières: pour regrouper une liste de chaînes par similarité uniquement, et pour regrouper une liste de nombres par contiguïté et similarité.
___
## Exemple de fichier

![List.GroupBySimilarity](./DSCore.List.GroupBySimilarity_img.jpg)
