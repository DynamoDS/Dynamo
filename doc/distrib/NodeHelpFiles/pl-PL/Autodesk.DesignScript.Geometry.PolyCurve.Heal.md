## Informacje szczegółowe
Węzeł `PolyCurve.Heal` pobiera samoprzecinającą się krzywą PolyCurve i zwraca nową krzywą PolyCurve, która nie jest samoprzecinająca się. Wejściowa krzywa PolyCurve może mieć maksymalnie 3 przecięcia własne. Oznacza to, że jeśli dowolny pojedynczy segment krzywej PolyCurve styka się lub przecina się z więcej niż 2 innymi segmentami, naprawa za pomocą tego węzła nie zadziała. Wprowadzenie wartości `trimLength` większej niż 0 spowoduje, że segmenty końcowe dłuższe niż `trimLength` nie zostaną ucięte.

W poniższym przykładzie samoprzecinająca się krzywa PolyCurve zostaje naprawiona za pomocą węzła `PolyCurve.Heal`.
___
## Plik przykładowy

![PolyCurve.Heal](./Autodesk.DesignScript.Geometry.PolyCurve.Heal_img.jpg)
