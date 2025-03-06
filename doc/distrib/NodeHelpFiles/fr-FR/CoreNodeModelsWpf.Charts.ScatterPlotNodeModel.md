## Description approfondie

L'option Diagramme de dispersion crée un graphique avec des points tracés par leurs valeurs X et Y et codés par couleur en fonction du groupe.
Étiquetez vos groupes ou modifiez le nombre de groupes en insérant une liste de valeurs de chaîne dans l'entrée des étiquettes. Chaque étiquette crée un groupe avec un code couleur correspondant. Si vous n'entrez qu'une seule valeur de chaîne, tous les points auront la même couleur et auront une étiquette partagée.

Pour déterminer la position de chaque point, utilisez une liste de listes contenant des valeurs doubles pour les entrées de valeurs X et Y. Les entrées de valeurs X et Y doivent contenir un nombre égal de valeurs. Le nombre de sous-listes doit également être aligné sur le nombre de valeurs de chaîne dans les entrées d'étiquettes.

Pour affecter une couleur à chaque groupe, insérez une liste de couleurs dans l'entrée des couleurs. Lors de l'affectation de couleurs personnalisées, le nombre de couleurs doit correspondre au nombre de valeurs de chaîne dans l'entrée des étiquettes. Si aucune couleur n'est affectée, des couleurs aléatoires seront utilisées.

___
## Exemple de fichier

![Scatter Plot](./CoreNodeModelsWpf.Charts.ScatterPlotNodeModel_img.jpg)

