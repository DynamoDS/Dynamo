## Description approfondie
ExtendWithArc ajoute un arc circulaire au début ou à la fin d'une PolyCurve d'entrée et renvoie une seule PolyCurve combinée. L'entrée de rayon détermine le rayon du cercle, tandis que l'entrée de longueur détermine la distance le long du cercle pour l'arc. La longueur totale doit être inférieure ou égale à la longueur d'un cercle complet avec le rayon donné. L'arc généré est tangent à la fin de la PolyCurve d'entrée. Une entrée booléenne pour endOrStart contrôle à quelle extrémité de la PolyCurve l'arc sera créé. Une valeur "True" entraîne la création de l'arc à la fin de la PolyCurve, tandis que "False" crée l'arc au début de la PolyCurve. Dans l'exemple ci-dessous, nous utilisons d'abord un ensemble de points aléatoires et PolyCurve ByPoints pour générer une PolyCurve. Nous utilisons ensuite deux curseurs numériques et une bascule booléenne pour définir les paramètres ExtendWithArc.
___
## Exemple de fichier

![ExtendWithArc](./Autodesk.DesignScript.Geometry.PolyCurve.ExtendWithArc_img.jpg)

