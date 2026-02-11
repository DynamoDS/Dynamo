## Description approfondie
NormalAtPoint recherche le vecteur normal de la surface au point d'entrée sur une surface. Si le point d'entrée n'est pas sur la surface, ce nœud trouvera le point sur la surface le plus proche du point d'entrée. Dans l'exemple ci-dessous, nous créons d'abord une surface en utilisant BySweep2Rails. Nous utilisons ensuite un Code Block pour spécifier un point où trouver la normale. Le point n'est pas sur la surface, alors le nœud utilise le point le plus proche comme position de recherche de la normale.
___
## Exemple de fichier

![NormalAtPoint](./Autodesk.DesignScript.Geometry.Surface.NormalAtPoint_img.jpg)

