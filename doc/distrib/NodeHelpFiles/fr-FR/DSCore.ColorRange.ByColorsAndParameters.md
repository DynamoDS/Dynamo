## Description approfondie
ByColorsAndParameters crée une plage de couleurs 2D à partir d'une liste de couleurs d'entrée et d'une liste correspondante de paramètres UV spécifiés dans un intervalle de 0 à 1. Dans l'exemple ci-dessous, nous utilisons un Code Block pour créer trois couleurs différentes (dans ce cas, simplement le vert, le rouge et le bleu) et les combiner dans une liste. Nous utilisons un Code Block distinct pour créer trois paramètres UV, un pour chaque couleur. Ces deux listes sont utilisées comme entrées pour un nœud ByColorsAndParameters. Nous utilisons ensuite un nœud GetColorAtParameter, ainsi qu'un nœud Display.ByGeometryColor pour visualiser la plage de couleurs 2D sur un ensemble de cubes.
___
## Exemple de fichier

![ByColorsAndParameters](./DSCore.ColorRange.ByColorsAndParameters_img.jpg)

