## Description approfondie
UVParameterAtPoint permet de trouver la position UV de la surface au point d'entrée sur une surface. Si le point d'entrée n'est pas sur la surface, ce nœud recherchera le point sur la surface le plus proche du point d'entrée. Dans l'exemple ci-dessous, nous créons d'abord une surface à l'aide d'un nœud BySweep2Rails. Nous utilisons ensuite un Code Block pour spécifier un point pour rechercher le paramètre UV. Le point n'est pas sur la surface, alors le nœud utilise le point le plus proche sur la surface comme position pour rechercher le paramètre UV.
___
## Exemple de fichier

![UVParameterAtPoint](./Autodesk.DesignScript.Geometry.Surface.UVParameterAtPoint_img.jpg)

