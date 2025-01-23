## Description approfondie
ContainmentTest renvoie une valeur booléenne indiquant si un point donné est contenu ou non à l'intérieur d'un polygone donné. Le polygone doit être plan et non auto-sécant pour que cela fonctionne. Dans l'exemple ci-dessous, nous créons un polygone à l'aide d'une série de points créés à l'aide de ByCylindricalCoordinates. Maintenir une élévation constante et trier les angles permet de créer un polygone plan et non auto-concourant. Nous créons ensuite un point à tester, puis nous utilisons ContainmentTest pour déterminer si le point se trouve à l'intérieur ou à l'extérieur du polygone.
___
## Exemple de fichier

![ContainmentTest](./Autodesk.DesignScript.Geometry.Polygon.ContainmentTest_img.jpg)

