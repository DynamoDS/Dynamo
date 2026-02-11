## Description approfondie
Le noeud 'Curve Mapper' redistribue une série de valeurs d'entrée comprises dans un intervalle défini et utilise les courbes mathématiques pour les mapper le long d'une courbe spécifiée. Dans ce contexte, le mappage consiste à redistribuer les valeurs de sorte que leurs coordonnées x suivent la forme de la courbe le long de l'axe des y. Cette technique est particulièrement utile pour des applications telles que la conception de façades, de structures de toit paramétriques et d'autres calculs de conception où des motifs ou des distributions spécifiques sont nécessaires.

Établissez les limites des coordonnées x en définissant les valeurs minimale et maximale. Ces limites définissent les délimitations à l'intérieur desquelles les points seront redistribués. Vous pouvez indiquer un nombre unique pour générer une série de valeurs réparties uniformément ou une série de valeurs existantes, qui seront réparties le long de la direction x dans l'intervalle spécifié puis mappées sur la courbe.

Sélectionnez une courbe mathématique parmi les options proposoées, notamment les courbes Linéaire, Sinus, Cosinus, Bruit de Perlin, Bézier, Gaussienne, Parabolique, Racine carrée et Puissance. Utilisez les points de contrôle interactifs pour ajuster la forme de la courbe sélectionnée, en l'adaptant à vos besoins précis.

Vous pouvez verrouiller la forme de la courbe à l'aide du bouton de verrouillage, afin d'empêcher que d'autres modifications y soient apportées. De plus, vous pouvez rétablir l'état par défaut de la forme à l'aide du bouton de réinitialisation à l'intérieur du noeud. Si vous obtenez les sorties NaN ou Null, vous trouverez davantage de détails sur les raisons de ces résultats [ici](https://dynamobim.org/introducing-the-curve-mapper-node-in-dynamo/#CurveMapper_Known_Issues).

Par exemple, pour redistribuer 80 points le long d'une courbe sinusoïdale dans l'intervalle de 0 à 20, définissez Min sur 0, Max sur 20 et Valeurs sur 80. Après avoir sélectionné la courbe sinusoïdale et ajusté sa forme selon vos besoins, le noeud 'Curve Mapper' génère 80 points, avec des coordonnées x qui suivent le motif de la courbe sinusoïdale le long de l'axe des y.

Pour mapper des valeurs réparties de façon non uniforme le long d'une courbe Gaussienne, définissez l'intervalle minimal et maximal, et indiquez la série de valeurs. Après avoir sélectionné la courbe Gaussienne et ajusté sa forme selon vos besoins, le noeud 'Curve Mapper' redistribue la série de valeurs le long des coordonnées x en utilisant l'intervalle spécifié et mappe les valeurs le long du motif de la courbe. Pour consulter une documentation détaillée sur le fonctionnement du noeud et sur la façon de définir les entrées, lisez [cet article de blog](https://dynamobim.org/introducing-the-curve-mapper-node-in-dynamo) sur Curve Mapper.




___
## Exemple de fichier

![Example](./GV5KUSHDGL7YVBZAR4HEGY5NIXFIG3XTV6ZQPHC5MWWGEVOSRJ4Q_img.png)
