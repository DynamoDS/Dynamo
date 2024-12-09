## Description approfondie
`PolyCurve.Heal` prend une PolyCurve auto-sécante en entrée et renvoie une nouvelle PolyCurve non auto-sécante. La PolyCurve d'entrée ne peut pas avoir plus de 3 points d'auto-intersection. En d'autres termes, si un segment de la PolyCurve entre en contact ou est sécant avec plus de 2 autres segments, la correction ne fonctionne pas. Si la valeur d'entrée `trimLength` est supérieure à 0, les segments d'extrémité plus longs que la valeur `trimLength` ne seront pas ajustés.

Dans l'exemple ci-dessous, une PolyCurve auto-sécante est corrigée à l'aide du noeud `PolyCurve.Heal`.
___
## Exemple de fichier

![PolyCurve.Heal](./Autodesk.DesignScript.Geometry.PolyCurve.Heal_img.jpg)
