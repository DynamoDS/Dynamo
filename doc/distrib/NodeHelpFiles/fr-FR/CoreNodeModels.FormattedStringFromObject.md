## Description approfondie
This node will convert an object to a string. The second input `format specifier` controls how numeric inputs are converted to their string representations.
Ce 'spécificateur de format' doit être l'un des spécificateurs numériques du format standard c#.

Les spécificateurs de format doivent se présenter sous la forme suivante:
'<specifier><precision>' par exemple F1

Voici quelques exemples de spécificateurs de format couramment utilisés:
```
G : formatage général G 1000.0 -> "1000"
F : notation point fixe F4 1000.0 -> "1000.0000"
N : numéro N2 1000 -> "1,000.00"
```

La valeur par défaut de ce nœud est 'G', ce qui génère une représentation compacte mais variable.

[Pour plus d'informations, reportez-vous à la documentation Microsoft.](https://learn.microsoft.com/fr-fr/dotnet/standard/base-types/standard-numeric-format-strings#standard-format-specifiers)
___
## Exemple de fichier

![Formatted String from Object](./CoreNodeModels.FormattedStringFromObject_img.jpg)
