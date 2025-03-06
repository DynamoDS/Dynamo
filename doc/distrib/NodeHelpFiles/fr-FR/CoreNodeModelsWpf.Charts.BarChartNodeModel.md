## Description approfondie

L'option Graphique à barres crée un graphique avec des barres orientées verticalement. Les barres peuvent être organisées sous plusieurs groupes et codées par couleur. Vous avez la possibilité de créer un seul groupe en saisissant une seule valeur double ou bien plusieurs groupes en saisissant plusieurs valeurs doubles pour chaque sous-liste dans les entrées des valeurs. Pour définir des catégories, insérez une liste de valeurs de chaînes dans les entrées d'étiquettes. Chaque valeur crée une nouvelle catégorie codée par couleur.

Pour attribuer une valeur (hauteur) à chaque barre, saisissez une liste de listes contenant des valeurs doubles dans les entrées des valeurs. Chaque sous-liste détermine le nombre de barres et la catégorie à laquelle elles appartiennent, dans le même ordre que les entrées des étiquettes. Si vous disposez d'une liste unique de valeurs doubles, , une seule catégorie sera créée. Le nombre de valeurs de chaînes dans les entrées des étiquettes doit être égal au nombre de sous-listes dans les entrées des valeurs.

Pour attribuer une couleur à chaque catégorie, insérez une liste de couleurs dans les entrées des couleurs. Lorsque vous attribuez des couleurs personnalisées, le nombre de couleurs doit être égal à celui des valeurs de chaînes dans les entrées d'étiquettes. Si aucune couleur n'est attribuée, des couleurs aléatoires seront utilisées.

## Exemple: un seul groupe

Imaginez que vous voulez représenter les évaluations moyennes des utilisateurs pour un élément au cours des trois premiers mois de l'année. Pour visualiser cela, vous avez besoin d'une liste de trois valeurs de chaîne, intitulées Janvier, Février et Mars.
Donc, pour l'entrée des étiquettes, nous allons fournir la liste suivante dans un Code Block :

["Janvier", "Février", "Mars"];

Vous pouvez également utiliser les nœuds de chaîne connectés au nœud de création de liste pour créer votre liste.

Ensuite, dans les valeurs entrées, nous allons entrer la moyenne des évaluations des utilisateurs pour chacun des trois mois sous forme de listes:

[3.5], [5], [4];

Notez que comme nous avons trois étiquettes, nous avons besoin de trois sous-listes.

Désormais, lorsque le graphique est exécuté, le graphique à barres est créé, chaque barre de couleur représentant le classement client moyen pour le mois. Vous pouvez continuer à utiliser les couleurs par défaut ou insérer une liste de couleurs personnalisées dans l'entrée des couleurs.

## Exemple: plusieurs groupes

Vous pouvez tirer parti de la fonctionnalité de regroupement du nœud Graphique à barres en entrant plus de valeurs dans chaque sous-liste de l'entrée de valeurs. Dans cet exemple, nous allons créer un graphique qui visualise le nombre de portes dans trois variantes de trois modèles: Modèle A, Modèle B et Modèle C.

Pour ce faire, nous allons d’abord fournir les étiquettes:

["Modèle A", "Modèle B", "Modèle C"];

Ensuite, nous allons fournir des valeurs, une fois de plus en nous assurant que le nombre de sous-listes correspond au nombre d'étiquettes:

[17, 9, 13],[12,11,15],[15,8,17]];

Désormais, lorsque vous cliquez sur Exécuter, le nœud Graphique à barres crée un graphique avec trois groupes de barres, marqués Index 0, 1 et 2, respectivement. Dans cet exemple, considérez chaque index (c'est-à-dire, groupe) comme une variation de conception. Les valeurs du premier groupe (Index 0) sont extraites du premier élément de chaque liste dans les valeurs entrées, de sorte que le premier groupe contient 17 pour le Modèle A, 12 pour le Modèle B et 15 pour le Modèle C. Le deuxième groupe (Index 1) utilise la deuxième valeur dans chaque groupe, et ainsi de suite.

___
## Exemple de fichier

![Bar Chart](./CoreNodeModelsWpf.Charts.BarChartNodeModel_img.jpg)

