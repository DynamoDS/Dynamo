## Description approfondie
'Solid.Repair' tente de réparer les solides dont la géométrie est incorrecte, ainsi que d'effectuer des optimisations potentielles. 'Solid.Repair' renvoie un nouvel objet solide.

Ce noeud est utile lorsque vous rencontrez des erreurs lors de l'exécution d'opérations sur une géométrie importée ou convertie.

Dans l'exemple ci-dessous, 'Solid.Repair' est utilisé pour réparer la géométrie provenant d'un fichier **. SAT**. La géométrie du fichier échoue lors d'une opération booléenne ou d'ajustement, et 'Solid.Repair' nettoie toute *géométrie incorrecte* à l'origine de l'échec.

En règle générale, vous ne devriez pas avoir besoin d'utiliser cette fonctionnalité sur les géométries que vous créez dans Dynamo, mais uniquement sur celles provenant de sources externes. Si vous constatez que ce n'est pas le cas, veuillez signaler un bogue à l'équipe Dynamo Github
___
## Exemple de fichier

![Solid.Repair](./Autodesk.DesignScript.Geometry.Solid.Repair_img.jpg)
