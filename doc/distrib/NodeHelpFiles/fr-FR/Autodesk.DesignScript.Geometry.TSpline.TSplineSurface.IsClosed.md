## Description approfondie
Une surface fermée est une surface qui forme une forme complète sans ouvertures ni limites.
Dans l'exemple ci-dessous, une sphère T-Spline générée via `TSplineSurface.BySphereCenterPointRadius` est inspectée à l'aide de `TSplineSurface.IsClosed` pour vérifier si elle est ouverte, ce qui renvoie un résultat négatif. En effet, les sphères de T-Spline, bien qu'elles semblent fermées, sont en réalité ouvertes aux pôles où plusieurs arêtes et sommets sont empilés en un seul point.

Les espaces dans la sphère de T-Spline sont ensuite remplis à l'aide du noeud `TSplineSurface.FillHole`, ce qui entraîne une légère déformation à l'endroit où la surface a été remplie. Lorsqu'elle est de nouveau vérifiée via le noeud `TSplineSurface.IsClosed`, elle génère désormais un résultat positif, ce qui signifie qu'elle est fermée.
___
## Exemple de fichier

![TSplineSurface.IsClosed](./Autodesk.DesignScript.Geometry.TSpline.TSplineSurface.IsClosed_img.jpg)
