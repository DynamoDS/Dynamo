## Informacje szczegółowe
Węzeł `List.RemoveIfNot` zwraca listę z zachowanymi elementami zgodnymi z danym typem elementu (type) i usuniętymi wszystkimi pozostałymi elementami z listy pierwotnej (list).

Może być konieczne użycie pełnej ścieżki węzła, takiej jak `Autodesk.DesignScript.Geometry.Surface`, w wartości wejściowej `type`, aby usunąć elementy. Aby pobrać ścieżki dla elementów listy, można wprowadzić listę do węzła `Object.Type`.

W poniższym przykładzie węzeł `List.RemoveIfNot` zwraca listę z jednym wierszem — elementy punktów z listy pierwotnej zostają usunięte, ponieważ nie są zgodne z określonym typem.
___
## Plik przykładowy

![List.RemoveIfNot](./List.RemoveIfNot_img.jpg)
