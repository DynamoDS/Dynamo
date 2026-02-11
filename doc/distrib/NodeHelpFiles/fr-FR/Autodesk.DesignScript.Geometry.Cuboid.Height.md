## Description approfondie
`Cuboid.Height` renvoie la hauteur du cuboïde d'entrée. Notez que si le cuboïde a été transformé en un système de coordonnées différent avec un facteur d'échelle, cela renvoie les cotes d'origine du cuboïde, et non les cotes de l'espace univers. En d'autres termes, si vous créez un cuboïde avec une largeur (axe X) de 10 et le transformez en CoordinateSystem avec une échelle de 2 en X, la largeur est toujours 10.

Dans l'exemple ci-dessous, nous générons un cuboïde par coins, puis nous utilisons un noeud `Cuboid.Height` pour trouver sa hauteur.

___
## Exemple de fichier

![Height](./Autodesk.DesignScript.Geometry.Cuboid.Height_img.jpg)

