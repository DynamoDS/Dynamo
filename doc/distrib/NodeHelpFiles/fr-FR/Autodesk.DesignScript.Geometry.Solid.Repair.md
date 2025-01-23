## Description approfondie
Repair tente de réparer les solides qui ont une géométrie incorrecte, ainsi que d'effectuer des optimisations potentielles. Le nœud Repair renvoie un nouvel objet solide.
Ce nœud est utile lorsque vous rencontrez des erreurs lors de l'exécution d'opérations sur une géométrie importée ou convertie.

Par exemple, si vous importez des données à partir d'un contexte hôte tel que **Revit** ou d'un fichier *.SAT** et que l'opération d'ajustement échoue ou ne parvient pas à fournir de valeur boléenne de manière inattendue, vous pouvez constater qu'une opération de réparation nettoie toute *géométrie invalide* à l'origine de l'échec.

En général, vous n'avez pas besoin d'utiliser cette fonctionnalité sur la géométrie que vous créez dans Dynamo, uniquement sur des géométries provenant de sources externes. Si vous trouvez que ce n'est pas le cas, veuillez signaler un bogue sur le github d'équipe Dynamo !
___


