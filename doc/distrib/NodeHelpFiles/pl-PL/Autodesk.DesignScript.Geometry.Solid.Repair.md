## Informacje szczegółowe
Węzeł `Solid.Repair` próbuje naprawić bryły, które mają nieprawidłową geometrię, jak również potencjalnie wykonać optymalizację. Węzeł `Solid.Repair` zwraca nowy obiekt bryły.

Ten węzeł jest przydatny w przypadku wystąpienia błędów podczas wykonywania operacji na zaimportowanej lub przekonwertowanej geometrii.

W poniższym przykładzie węzeł `Solid.Repair` jest używany do naprawy geometrii z pliku **.SAT**. Geometria w pliku nie poddaje się przetworzeniu logicznemu ani przycięciu, więc węzeł `Solid.Repair` oczyszcza *nieprawidłową geometrię*, która powoduje te błędy.

Z reguły nie trzeba używać tej funkcji w przypadku geometrii utworzonej w dodatku Dynamo, a jedynie w przypadku geometrii ze źródeł zewnętrznych. Jeśli okaże się, że tak nie jest, zgłoś błąd zespołowi dodatku Dynamo w serwisie GitHub
___
## Plik przykładowy

![Solid.Repair](./Autodesk.DesignScript.Geometry.Solid.Repair_img.jpg)
