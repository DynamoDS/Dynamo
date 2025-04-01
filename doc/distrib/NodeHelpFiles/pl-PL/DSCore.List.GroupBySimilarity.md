## Informacje szczegółowe
Węzeł `List.GroupBySimilarity` grupuje elementy listy na podstawie sąsiadowania indeksów i podobieństwa ich wartości. Lista elementów do zgrupowania może zawierać liczby (liczby całkowite i zmiennoprzecinkowe) lub ciągi, ale nie może zawierać kombinacji obu typów.

Użyj wartości wejściowej `tolerance`, aby określić podobieństwo elementów. W przypadku list liczb wartość 'tolerance' reprezentuje maksymalną dopuszczalną różnicę między dwiema liczbami, które można uznać za podobne.

For lists of strings, this value (between 0 and 10) represents the minimum similarity ratio (computed using fuzzy logic) for adjacent elements to be considered similar. For string lists, 'tolerance' represents the maximum number of characters that can differ between two strings, using Levenshtein distance for comparison. Maximum tolerance for strings is set to 10.

Wejściowa wartość logiczna (Boolean) `considerAdjacency` wskazuje, czy sąsiadowanie powinno być uwzględniane podczas tworzenia grup elementów. Jeśli wartością jest True (Prawda), zgrupowane zostaną tylko sąsiadujące elementy, które są podobne. Jeśli wartością jest False (Fałsz), do tworzenia klastrów zostanie użyte samo podobieństwo, niezależnie od sąsiadowania.

Węzeł zwraca listę list wartości zgrupowanych na podstawie sąsiadowania i podobieństwa, jak również listę list indeksów zgrupowanych elementów na liście pierwotnej.

W poniższym przykładzie węzeł `List.GroupBySimilarity` jest używany na dwa sposoby: do grupowania listy ciągów tylko na podstawie podobieństwa i do grupowania listy liczb na podstawie sąsiadowania i podobieństwa.
___
## Plik przykładowy

![List.GroupBySimilarity](./DSCore.List.GroupBySimilarity_img.jpg)
