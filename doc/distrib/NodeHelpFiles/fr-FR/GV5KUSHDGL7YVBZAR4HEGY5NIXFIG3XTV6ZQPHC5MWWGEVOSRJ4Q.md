## Description approfondie
The ‘Curve Mapper’ node leverages mathematical curves to redistribute points within a defined range. Redistribution in this context means reassigning x-coordinates to new positions along a specified curve based on their y-coordinates. This technique is particularly valuable for applications such as façade design, parametric roof structures, and other design calculations where specific patterns or distributions are required.

Établissez les limites des coordonnées x et y en définissant les valeurs minimale et maximale. Ces limites définissent les délimitations à l'intérieur desquelles les points seront redistribués. Ensuite, sélectionnez une courbe mathématique parmi les options proposées, notamment les courbes Linéaire, Sinus, Cosinus, Bruit de Perlin, Bézier, Gaussienne, Parabolique, Racine carrée et Puissance. Utilisez les points de contrôle interactifs pour ajuster la forme de la courbe sélectionnée, en l'adaptant à vos besoins précis.

Vous pouvez verrouiller la forme de la courbe à l'aide du bouton de verrouillage, ce qui empêche toute modification ultérieure de la courbe. En outre, vous pouvez rétablir l'état par défaut de la forme à l'aide du bouton de réinitialisation à l'intérieur du noeud.

Spécifiez le nombre de points à redistribuer en définissant l'entrée Nombre. Le noeud calcule de nouvelles coordonnées x pour le nombre spécifié de points en fonction de la courbe sélectionnée et des limites définies. Les points sont redistribués de sorte que leurs coordonnées x suivent la forme de la courbe le long de l'axe y.

For example, to redistribute 80 points along a sine curve, set Min X to 0, Max X to 20, Min Y to 0, and Max Y to 10. After selecting the sine curve and adjusting its shape as needed, the ‘Curve Mapper’ node outputs 80 points with x-coordinates that follow the sine curve pattern along the y-axis from 0 to 10.


___
## Exemple de fichier


