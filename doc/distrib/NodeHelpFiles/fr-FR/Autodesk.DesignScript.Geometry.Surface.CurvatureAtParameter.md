## Description approfondie
CurvatureAtParameter utilise les paramètres d'entrée U et V et renvoie un système de coordonnées basé sur la normale, la direction U et la direction V à la position UV sur la surface. Le vecteur normal détermine l'axe Z, tandis que les directions U et V déterminent la direction des axes X et Y. La longueur des axes est déterminée par la courbure U et V. Dans l'exemple ci-dessous, nous créons d'abord une surface à l'aide de BySweep2Rails. Nous utilisons ensuite deux curseurs numériques pour déterminer les paramètres U et V afin de créer un système de coordonnées avec un nœud CurvatureAtParameter.
___
## Exemple de fichier

![CurvatureAtParameter](./Autodesk.DesignScript.Geometry.Surface.CurvatureAtParameter_img.jpg)

