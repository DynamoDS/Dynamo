## Description approfondie
`List.IsUniformDepth` renvoie une valeur booléenne indiquant si la profondeur de la liste est cohérente ou non, ce qui signifie que chaque liste contient le même nombre de listes imbriquées ou non.

Dans l'exemple ci-dessous, deux listes sont comparées, une de profondeur uniforme et une de profondeur non uniforme, afin de montrer la différence. La liste uniforme contient trois listes sans listes imbriquées. La liste non uniforme contient deux listes. La première liste ne présente pas de listes imbriquées, mais la seconde en a deux. Les listes à [0] et [1] ne sont pas égales en profondeur, donc `List.IsUniformDepth` renvoie False.
___
## Exemple de fichier

![List.IsUniformDepth](./DSCore.List.IsUniformDepth_img.jpg)
