## Description approfondie
FromPixels avec width et height crée une image à partir d'une liste simple de couleurs d'entrée, où chaque couleur devient un pixel. La largeur (width) multipliée par la hauteur (height) doit être égale au nombre total de couleurs. Dans l'exemple ci-dessous, nous créons d'abord une liste de couleurs à l'aide d'un nœud ByARGB. Un Code Block crée une plage de valeurs de 0 à 255, qui, une fois connecté aux entrées r et g, produit une série de couleurs allant du noir au jaune. Nous créons une image d'une largeur de 8. Un nœud Count et un nœud Division sont utilisés pour déterminer la hauteur de l'image. Un nœud Watch Image peut être utilisé pour afficher un aperçu de l'image créée.
___
## Exemple de fichier

![FromPixels (colors, width, height)](./DSCore.IO.Image.FromPixels(colors,%20width,%20height)_img.jpg)

