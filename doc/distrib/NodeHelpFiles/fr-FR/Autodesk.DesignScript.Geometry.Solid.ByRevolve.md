## Description approfondie
`Solid.ByRevolve` crée une surface en faisant pivoter une courbe de profil donnée autour d'un axe. L'axe est défini par un point `axisOrigin` et un vecteur `axisDirection`. Le `startAngle` détermine l'endroit où commencer la surface, mesuré en degrés, et le `sweepAngle` détermine la distance autour de l'axe pendant laquelle la surface continue.

Dans l'exemple ci-dessous, nous utilisons une courbe générée avec une fonction cosinus comme courbe de profil ainsi que deux curseurs numériques pour contrôler le `startAngle` et le `sweepAngle`. Pour cet exemple, les valeurs `axisOrigin` et `axisDirection` sont les valeurs par défaut de l'origine et de l'axe Z universels.

___
## Exemple de fichier

![ByRevolve](./Autodesk.DesignScript.Geometry.Solid.ByRevolve_img.jpg)

