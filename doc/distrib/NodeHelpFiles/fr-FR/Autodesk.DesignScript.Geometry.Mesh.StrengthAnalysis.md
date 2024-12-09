## Détails
 Le noeud 'Mesh.StrengthAnalysis' renvoie une liste de couleurs représentatives pour chaque sommet. Le résultat peut être utilisé avec le noeud 'Mesh.ByMeshColor'. Les zones les plus résistantes du maillage sont colorées en vert tandis que les zones les plus faibles sont indiquées par une carte thermique allant du jaune au rouge. L'analyse peut entraîner des faux positifs si le maillage est trop grossier ou irrégulier (c'est-à-dire s'il comporte de nombreux triangles longs et fins). Vous pouvez essayer d'utiliser 'Mesh.Remesh' pour générer un maillage régulier avant d'appeler 'Mesh.StrengthAnalysis' pour générer de meilleurs résultats.

Dans l'exemple ci-dessous, 'Mesh.StrengthAnalysis' est utilisé pour appliquer un code couleur à la résistance structurelle d'un maillage en forme de grille. Cela donne une liste de couleurs correspondant à la longueur des sommets du maillage. Cette liste peut être utilisée avec le noeud 'Mesh.ByMeshColor' pour colorer le maillage.

## Exemple de fichier

![Example](./Autodesk.DesignScript.Geometry.Mesh.StrengthAnalysis_img.jpg)
