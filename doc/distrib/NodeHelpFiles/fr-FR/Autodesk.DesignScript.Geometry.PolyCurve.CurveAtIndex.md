## Description approfondie
CurveAtIndex renvoie le segment de courbe à l'index d'entrée d'une PolyCurve donnée. Si le nombre de courbes de la PolyCurve est inférieur à l'index donné, CurveAtIndex renvoie une valeur nulle. L'entrée endOrStart accepte une valeur booléenne True ou False. Si la valeur est True, CurveAtIndex commence à compter au niveau du premier segment de la PolyCurve. Si la valeur est False, le nombre est compté à reculons à partir du dernier segment. Dans l'exemple ci-dessous, nous générons un ensemble de points aléatoires, puis nous utilisons PolyCurve ByPoints pour créer une PolyCurve ouverte. Nous pouvons ensuite utiliser CurveAtIndex pour extraire des segments spécifiques de la PolyCurve.
___
## Exemple de fichier

![CurveAtIndex](./Autodesk.DesignScript.Geometry.PolyCurve.CurveAtIndex_img.jpg)

