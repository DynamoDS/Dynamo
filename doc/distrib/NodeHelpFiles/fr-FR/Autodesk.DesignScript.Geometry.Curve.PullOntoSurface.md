## Description approfondie
PullOntoSurface permet de créer une nouvelle courbe en projetant une courbe d'entrée sur une surface d'entrée, en utilisant les vecteurs normaux de la surface comme directions de projection. Dans l'exemple ci-dessous, nous créons d'abord une surface à l'aide d'un nœud Surface.BySweep qui utilise des courbes générées selon une courbe sinusoïdale. Cette surface est utilisée comme surface de base sur laquelle transférer un nœud PullOntoSurface. Pour la courbe, nous créons un cercle à l'aide d'un Code Block pour spécifier les coordonnées du point central et un curseur numérique pour contrôler le rayon du cercle. Le résultat obtenu est une projection du cercle sur la surface.
___
## Exemple de fichier

![PullOntoSurface](./Autodesk.DesignScript.Geometry.Curve.PullOntoSurface_img.jpg)

