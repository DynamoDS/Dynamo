## Description approfondie
FromPixels crée un objet image à partir d'un réseau de couleurs bidimensionnel d'entrée. Dans l'exemple ci-dessous, nous utilisons d'abord un Code Block pour générer une plage de nombres de 0 à 255. Un nœud Color.ByARGB est utilisé pour créer des couleurs à partir de cette plage et la combinaison de ce nœud est définie sur Produit cartésien pour créer un réseau bidimensionnel. Ensuite, nous utilisons un nœud Image.FromPixels afin de créer une image. Le nœud Watch Image peut être utilisé pour afficher un aperçu de l'image créée.
___
## Exemple de fichier

![FromPixels (colors)](./DSCore.IO.Image.FromPixels(colors)_img.jpg)

