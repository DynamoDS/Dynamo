## Description approfondie

Le noeud Define Data valide le type des données entrantes. Il peut être utilisé pour s'assurer que les données locales sont du type souhaité et peut également être désigné comme noeud d'entrée ou de sortie, en indiquant le type de données qu'un graphique attend ou fournit. Le noeud prend en charge une sélection de types de données Dynamo courants, par exemple "String", "Point" ou "Boolean". La liste complète des types de données pris en charge est disponible dans le menu déroulant du noeud. Le noeud prend en charge les données sous forme de valeur unique ou de liste simple. Les listes imbriquées, les dictionnaires et la réplication ne sont pas pris en charge.

### Comportement

Le noeud valide les données provenant du port d'entrée en fonction des paramètres du menu déroulant Type et du bouton bascule List (voir ci-dessous pour plus de détails). Si la validation réussit, la sortie du noeud est identique à celle de l'entrée. Si la validation échoue, le noeud passe en état d'avertissement et présente une sortie nulle.
Le noeud comporte une entrée :

-   Entrée "**>**" : se connecte à un noeud en amont pour valider le type de ses données.
    Le noeud propose également trois contrôles utilisateur :
-   Bouton bascule **Auto-detect type** : lorsque cette option est activée, le noeud analyse les données entrantes. Si le type de données est pris en charge, le noeud définit les valeurs de contrôle Type et List en fonction du type des données entrantes. Le menu déroulant Type et le bouton bascule List sont désactivés et automatiquement mis à jour en fonction du noeud d'entrée.

    Lorsque le bouton bascule Auto-detect type est désactivé, vous pouvez spécifier un type de données à l'aide du menu Type et du bouton bascule List. Si les données entrantes ne correspondent pas au type spécifié, le noeud passe en état d'avertissement et présente une sortie nulle.
-   Menu déroulant **Type** : définit le type de données attendu. Lorsque le contrôle est activé (bouton bascule **Auto-detect type** désactivé), définit un type de données pour la validation. Lorsque le contrôle est désactivé (bouton bascule **Auto-detect type** activé), le type de données est automatiquement défini en fonction des données entrantes. Les données sont valides si leur type correspond exactement au type affiché ou s'il s'agit d'un type enfant (par exemple, si le menu déroulant Type est défini sur "Curve", les objets de type "Rectangle", "Line", etc. sont valides).
- Bouton bascule **List** : lorsque cette option est activée, le noeud attend des données entrantes qu'elles soient sous forme de liste simple contenant des éléments d'un type de données valide (voir ci-dessus). Lorsqu'elle est désactivée, le noeud attend un élément unique d'un type de données valide.

### Utiliser comme noeud d'entrée

Lorsqu'il est défini comme entrée ("Is Input" dans le menu contextuel du noeud), le noeud peut utiliser en option des noeuds en amont afin de définir la valeur par défaut de l'entrée. Une exécution de graphique permet de mettre en cache la valeur du noeud Define Data en vue de l'utiliser lors de l'exécution externe du graphique, par exemple dans le noeud Engine.

## Exemple de fichier

Dans l'exemple ci-dessous, le bouton bascule **Auto-detect type** est désactivé pour le premier groupe de noeuds "DefineData". Le noeud valide correctement l'entrée Number fournie lors du rejet de l'entrée String. Le deuxième groupe contient un noeud présentant un bouton bascule **Auto-detect type* activé. Le noeud ajuste automatiquement la liste déroulante Type et le bouton bascule List pour correspondre à l'entrée, dans ce cas une liste de nombres entiers.

![Define_Data](./CoreNodeModels.DefineData_img.png)
