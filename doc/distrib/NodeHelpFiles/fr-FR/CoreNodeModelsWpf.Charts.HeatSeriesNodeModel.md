## Description approfondie

Le tracé de série de chaleur crée un graphique dans lequel les points de données sont représentés sous forme de rectangles de différentes couleurs le long d'une plage de couleurs.

Attribuez des étiquettes à chaque colonne et ligne en saisissant une liste des étiquettes de chaîne dans les saisies des étiquettes x et y, respectivement. Le nombre d'étiquettes x et y ne doit pas nécessairement concorder.

Attribuez une valeur à chaque rectangle. Le nombre de sous-listes doit concorder avec le nombre de valeurs de chaînes dans la saisie des étiquettes x, car il représente le nombre de colonnes. Les valeurs dans chaque sous-liste représentent le nombre de rectangles de chaque colonne. Par exemple, 4 sous-listes correspondent à 4 colonnes et si chaque sous-liste comprend 5 valeurs, les colonnes possèdent 5 rectangles chacune.

À titre d'exemple, pour créer une grille avec 5 lignes t 5 colonnes, renseignez 5 valeurs de chaînes dans les étiquettes x ainsi que les étiquettes y. Les valeurs des étiquettes y s'affichera sous le graphique le long de l'axe des abscisses (x), et les valeurs des étiquettes y s'affichera sur la gauche du graphique le long de l'axe des ordonnées (y).

Dans la saisie des valeurs, entrez une liste de listes, chaque sous-liste contenant 5 valeurs. Les valeurs sont tracées colonne par colonne de gauche à droite et de bas en haut, de sorte que la première valeur de la première sous-liste est le rectangle inférieur dans la colonne de gauche, la seconde valeur est le rectangle au-dessus de celui-ci, et ainsi de suite. Chaque sous-liste représente une colonne dans le tracé.

Vous pouvez affecter une plage de couleurs pour différencier les points de données en entrant une liste de valeurs de couleur dans les couleurs entrées. La valeur la plus basse du graphique sera égale à la première couleur et la valeur la plus haute sera égale à la dernière couleur, avec d'autres valeurs entre les deux le long du dégradé. Si aucune plage de couleurs n'est affectée, les points de données se verront attribuer une couleur aléatoire allant de la plus claire à la plus sombre.

Pour obtenir de meilleurs résultats, utilisez une ou deux couleurs. Le fichier exemple fournit un exemple classique de deux couleurs, le bleu et le rouge. Lorsqu'elles sont utilisées comme entrées de couleur, le tracé de série de chaleur crée automatiquement un dégradé entre ces couleurs, avec des valeurs faibles représentées dans des nuances de bleu et des valeurs élevées dans des nuances de rouge.

___
## Exemple de fichier

![Heat Series Plot](./CoreNodeModelsWpf.Charts.HeatSeriesNodeModel_img.jpg)

