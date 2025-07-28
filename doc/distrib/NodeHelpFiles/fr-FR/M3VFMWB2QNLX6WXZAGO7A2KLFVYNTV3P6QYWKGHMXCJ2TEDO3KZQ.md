## In-Depth
`TSplineSurface.BuildPipes` génère une surface de canalisation de T-Spline à l'aide d'un réseau de courbes. Les canalisations sont considérées comme étant jointes si leurs extrémités se trouvent dans la tolérance maximale définie par l'entrée `snappingTolerance`. Le résultat de ce noeud peut être affiné avec un jeu d'entrées qui permet de définir les valeurs de toutes les canalisations ou de chaque canalisation, si l'entrée est une liste de longueur égale au nombre de canalisations. Les entrées suivantes peuvent être utilisées de cette façon : `segmentsCount`, `startRotations`, `endRotations`, `startRadii`, `endRadii`, `startPositions` et `endPositions`.

Dans l'exemple ci-dessous, trois courbes jointes aux extrémités sont fournies en entrée pour le noeud `TSplineSurface.BuildPipes`. Dans ce cas, le `defaultRadius` est une valeur unique pour les trois canalisations, définissant le rayon des canalisations par défaut, sauf si les rayons de début et de fin sont fournis.
Ensuite, `segmentsCount` définit trois valeurs différentes pour chaque canalisation individuelle. L'entrée est une liste de trois valeurs, chacune correspondant à une canalisation.

D'autres réglages deviennent disponibles si `autoHandleStart` et `autoHandleEnd` sont définis sur False. Cela permet de contrôler les rotations de départ et de fin de chaque canalisation (entrées`startRotations` et `endRotations`), ainsi que les rayons à la fin et au début de chaque canalisation, en spécifiant les entrées `startRadii` et `endRadii`. Enfin, `startPositions` et `endPositions` permettent de décaler les segments au début ou à la fin de chaque courbe, respectivement. Cette entrée attend une valeur correspondant au paramètre de la courbe à l'emplacement où les segments commencent ou se terminent (valeurs comprises entre 0 et 1).

## Exemple de fichier
![Example](./M3VFMWB2QNLX6WXZAGO7A2KLFVYNTV3P6QYWKGHMXCJ2TEDO3KZQ_img.gif)
