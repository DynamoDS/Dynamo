<!--- Autodesk.DesignScript.Geometry.NurbsCurve.ByControlPointsWeightsKnots --->
<!--- T6GEU2COB3ZCMHPIT6WYQEY7NOLFALMOFIPSGLNKU5GNGESBEB7Q --->
## Description approfondie
`NurbsCurve.ByControlPointsWeightsKnots` permet de contrôler manuellement les poids et les noeuds d'une NurbsCurve. La liste des poids doit être de la même longueur que la liste des points de contrôle. La taille de la liste de noeuds doit être égale au nombre de points de contrôle plus le degré plus 1.

Dans l'exemple ci-dessous, nous créons d'abord une NurbsCurve par interpolation entre une série de points aléatoires. Nous utilisons des noeuds, des poids et des points de contrôle pour trouver les parties correspondantes de cette courbe. Nous pouvons utiliser le noeud `List.ReplaceItemAtIndex` pour modifier la liste des poids. Enfin, nous utilisons `NurbsCurve.ByControlPointsWeightsKnots` pour recréer une NurbsCurve avec les poids modifiés.

___
## Exemple de fichier

![ByControlPointsWeightsKnots](./T6GEU2COB3ZCMHPIT6WYQEY7NOLFALMOFIPSGLNKU5GNGESBEB7Q_img.jpg)

