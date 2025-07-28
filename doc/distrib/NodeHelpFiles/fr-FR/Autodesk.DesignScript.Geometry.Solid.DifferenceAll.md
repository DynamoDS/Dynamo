## Description approfondie
DifferenceAll crée un nouveau solide en soustrayant une liste de solides d'un seul solide. L'entrée "solid" indique le solide à partir duquel soustraire, tandis que l'entrée "tools" correspond à la liste des solides qui seront soustraits. Les solides de cette liste sont réunis pour créer un solide unique, qui est ensuite soustrait de l'entrée "solid". Dans l'exemple ci-dessous, nous commençons avec un cube par défaut comme le solide à partir duquel nous allons soustraire. Nous utilisons une série de curseurs numériques pour contrôler la position et le rayon d'une sphère. En utilisant une séquence de nombres comme coordonnée Z, nous créons une liste de plusieurs sphères. Si les sphères croisent le cube, nous obtenons un cube dont les parties sécantes des sphères sont soustraites.
___
## Exemple de fichier

![DifferenceAll](./Autodesk.DesignScript.Geometry.Solid.DifferenceAll_img.jpg)

