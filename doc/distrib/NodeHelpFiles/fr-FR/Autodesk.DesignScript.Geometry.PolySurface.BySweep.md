## Description approfondie
`PolySurface.BySweep (rail, crossSection)` renvoie une PolySurface en balayant une liste de lignes connectées et non sécantes le long d'une traverse. L'entrée `crossSection` peut recevoir une liste de courbes connectées qui doivent se rejoindre à un point de départ ou d'arrivée, sinon le noeud ne renvoie pas de PolySurface. Ce noeud est similaire à `PolySurface.BySweep (rail, profile)`, à la seule différence que le champ `crossSection` prend une liste de courbes en entrée alors que le champ `profile` ne prend qu'une seule courbe en entrée.

Dans l'exemple ci-dessous, une PolySurface est créée par balayage le long d'un arc.


___
## Exemple de fichier

![PolySurface.BySweep](./Autodesk.DesignScript.Geometry.PolySurface.BySweep_img.jpg)
