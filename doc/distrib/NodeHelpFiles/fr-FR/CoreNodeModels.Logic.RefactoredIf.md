## Description approfondie
If agit comme un nœud de contrôle conditionnel. L'entrée "test" prend une valeur booléenne, tandis que les entrées "true" et "false" peuvent accepter n'importe quel type de données. Si la valeur de test est "true", le nœud renvoie l'élément à partir de l'entrée "true", si le test est "false", le nœud renvoie l'élément à partir de l'entrée "false". Dans l'exemple ci-dessous, nous générons d'abord une liste de nombres aléatoires entre 0 et 99. Le nombre d'éléments dans la liste est contrôlé par un curseur de nombres entiers. Nous utilisons un Code Block avec la formule "x%a==0" pour tester la divisibilité par un deuxième nombre, déterminé par un second curseur de nombres entiers. Cela génère une liste de valeurs booléennes correspondant à la division des éléments de la liste aléatoire par le nombre déterminé par le deuxième curseur. Cette liste de valeurs booléennes est utilisée comme entrée "test" pour un nœud If? Nous utilisons une sphère par défaut comme entrée "true" et un cuboïde par défaut comme entrée "false". Le résultat du nœud If est une liste de sphères ou de cuboïdes. Enfin, nous utilisons un nœud Translate pour répartir la liste des géométries.

If est répliqué sur tous les nœuds AS THOUGH SET TO SHORTEST. Vous pouvez en voir la raison dans les exemples ci-joints, notamment lorsque vous consultez les résultats où LONGEST est appliqué à un nœud de formule et que la branche "short" de la condition est transmise. Ces modifications ont également été apportées pour permettre un comportement prévisible lors de l'utilisation d'entrées booléennes uniques ou d'une liste de valeurs booléennes.
___
## Exemple de fichier

![If](./CoreNodeModels.Logic.RefactoredIf_img.jpg)

