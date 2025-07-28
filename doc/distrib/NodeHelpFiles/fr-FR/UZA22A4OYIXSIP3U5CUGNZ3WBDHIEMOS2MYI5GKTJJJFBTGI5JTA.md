<!--- Autodesk.DesignScript.Geometry.TSpline.TSplineSurface.WeldCoincidentVertices --->
<!--- UZA22A4OYIXSIP3U5CUGNZ3WBDHIEMOS2MYI5GKTJJJFBTGI5JTA --->
## Description approfondie
Dans l'exemple ci-dessous, deux moitiés d'une surface de T-Spline sont combinées à l'aide du noeud `TSplineSurface.ByCombinedTSplineSurfaces`. Les sommets le long du plan miroir se chevauchent, ce qui devient visible lorsque l'un des sommets est déplacé à l'aide du noeud `TSplineSurface.MoveVertices`. Pour corriger cela, le noeud 'TSplineSurface.WeldCoincidentVertices` est utilisé pour réaliser la soudure. Le résultat du déplacement d'un sommet est désormais différent, et converti sur le côté pour obtenir un meilleur aperçu.
___
## Exemple de fichier

![TSplineSurface.WeldCoincidentVertices](./UZA22A4OYIXSIP3U5CUGNZ3WBDHIEMOS2MYI5GKTJJJFBTGI5JTA_img.jpg)
