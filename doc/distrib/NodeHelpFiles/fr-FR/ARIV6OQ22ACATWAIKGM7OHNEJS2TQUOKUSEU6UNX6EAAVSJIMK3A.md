<!--- Autodesk.DesignScript.Geometry.TSpline.TSplineSurface.CompressIndexes --->
<!--- ARIV6OQ22ACATWAIKGM7OHNEJS2TQUOKUSEU6UNX6EAAVSJIMK3A --->
## Description approfondie
Le noeud `TSplineSurface.CompressIndexes` supprime les espaces dans les numéros d'index des arêtes, des sommets ou des faces d'une surface de T-Spline résultant de diverses opérations telles que la suppression de face. L'ordre des index est conservé.

Dans l'exemple ci-dessous, un certain nombre de faces est supprimé d'une surface primitive de quadball qui affecte les index d'arête de la forme. `TSplineSurface.CompressIndexes` est utilisé pour réparer les index d'arête de la forme. Ainsi, la sélection d'une arête avec l'index 1 devient possible.

## Exemple de fichier

![Example](./ARIV6OQ22ACATWAIKGM7OHNEJS2TQUOKUSEU6UNX6EAAVSJIMK3A_img.jpg)
