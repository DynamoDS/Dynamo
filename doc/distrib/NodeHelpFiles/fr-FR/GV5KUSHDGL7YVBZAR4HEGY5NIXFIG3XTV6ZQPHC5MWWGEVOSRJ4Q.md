## Description approfondie
Le noeud 'Curve Mapper' utilise les courbes mathématiques pour redistribuer les points dans une plage définie. Dans ce contexte, la redistribution consiste à réattribuer les coordonnées x à de nouvelles positions le long d'une courbe spécifiée en fonction de leurs coordonnées y. Cette technique est particulièrement utile pour des applications telles que la conception de façades, de structures de toit paramétriques et d'autres calculs de conception où des motifs ou des distributions spécifiques sont nécessaires.

Établissez les limites des coordonnées x et y en définissant les valeurs minimale et maximale. Ces limites définissent les délimitations à l'intérieur desquelles les points seront redistribués. Ensuite, sélectionnez une courbe mathématique parmi les options proposées, notamment les courbes Linéaire, Sinus, Cosinus, Bruit de Perlin, Bézier, Gaussienne, Parabolique, Racine carrée et Puissance. Utilisez les points de contrôle interactifs pour ajuster la forme de la courbe sélectionnée, en l'adaptant à vos besoins précis.

Vous pouvez verrouiller la forme de la courbe à l'aide du bouton de verrouillage, ce qui empêche toute modification ultérieure de la courbe. En outre, vous pouvez rétablir l'état par défaut de la forme à l'aide du bouton de réinitialisation à l'intérieur du noeud.

Spécifiez le nombre de points à redistribuer en définissant l'entrée Nombre. Le noeud calcule de nouvelles coordonnées x pour le nombre spécifié de points en fonction de la courbe sélectionnée et des limites définies. Les points sont redistribués de sorte que leurs coordonnées x suivent la forme de la courbe le long de l'axe y.

Par exemple, pour redistribuer 80 points le long d'une courbe sinusoïdale, définissez Min X sur 0, Max X sur 20, Min Y sur 0 et Max Y sur 10. Après avoir sélectionné la courbe sinusoïdale et ajusté sa forme selon vos besoins, le noeud 'Curve Mapper' génère 80 points avec des coordonnées x qui suivent le motif de la courbe sinusoïdale le long de l'axe des y de 0 à 10.




___
## Exemple de fichier

![Example](./GV5KUSHDGL7YVBZAR4HEGY5NIXFIG3XTV6ZQPHC5MWWGEVOSRJ4Q_img.jpg)
