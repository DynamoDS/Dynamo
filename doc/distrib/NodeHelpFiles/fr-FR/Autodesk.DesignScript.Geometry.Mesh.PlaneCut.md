## Détails
'Mesh.PlaneCut' renvoie un maillage qui a été coupé par un plan donné. Le résultat de la coupe est la partie du maillage qui se trouve sur le côté du plan dans la direction de la normale de l'entrée 'plane'. Le paramètre 'makeSolid' contrôle si le maillage est traité comme un 'solide', auquel cas la coupe est remplie avec le moins de triangles possible pour couvrir chaque trou.

Dans l'exemple ci-dessous, un maillage creux obtenu à partir d'une opération 'Mesh.BooleanDifference' est coupé par un plan à un angle.

## Exemple de fichier

![Example](./Autodesk.DesignScript.Geometry.Mesh.PlaneCut_img.jpg)
