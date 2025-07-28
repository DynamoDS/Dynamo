## Description approfondie
GetColorAtParameter prend une plage de couleurs 2D d'entrée et renvoie une liste de couleurs aux paramètres UV spécifiés dans un intervalle de 0 à 1. Dans l'exemple ci-dessous, nous créons d'abord une plage de couleurs 2D à l'aide d'un nœud ByColorsAndParameters avec une liste de couleurs et une liste de paramètres de définition de la plage. Un Code Block est utilisé pour générer un intervalle de nombres entre 0 et 1, qui est utilisé comme entrées u et v dans un nœud UV.ByCoordinates. La combinaison de ce nœud est définie sur produit cartésien. Un ensemble de cubes est créé de manière similaire à un nœud Point.ByCoordinates avec une combinaison de produits cartésiens utilisée pour créer un réseau de cubes. Nous utilisons ensuite un nœud Display.ByGeometryColor avec le réseau de cubes et la liste des couleurs obtenues à partir du nœud GetColorAtParameter.
___
## Exemple de fichier

![GetColorAtParameter](./DSCore.ColorRange.GetColorAtParameter_img.jpg)

