## Informacje szczegółowe
Węzeł `Geometry.ImportFromSATWithUnits` importuje geometrię do dodatku Dynamo z pliku .SAT i z `DynamoUnit.Unit` z możliwością przekonwertowania z milimetrów. Ten węzeł pobiera obiekt pliku lub ścieżkę pliku jako pierwszą pozycję wejściową i jednostkę `dynamoUnit` jako drugą. W przypadku pozostawienia pozycji danych wejściowych `dynamoUnit` o wartości null geometria .SAT zostanie zaimportowana jako niemianowana — dane geometrii z pliku zostaną zaimportowane bez konwersji jednostek. W przypadku podania jednostki jednostki wewnętrzne pliku .SAT zostaną przekonwertowane na określone jednostki.

Dodatek Dynamo jest niemianowany (bez jednostek), ale wartości liczbowe na wykresie Dynamo prawdopodobnie mają pewne jednostki niejawne. Za pomocą pozycji wejściowej `dynamoUnit` można przeskalować geometrię wewnętrzną pliku .SAT do tego systemu jednostek.

W poniższym przykładzie z pliku .SAT zostaje zaimportowana geometria, a jednostką są stopy. Aby ten plik przykładowy działał na komputerze, pobierz ten przykładowy plik SAT i wskaż w węźle `File Path` plik invalid.sat.

___
## Plik przykładowy

![Geometry.ImportFromSATWithUnits](./GeometryUI.ImportFromSATWithUnits_img.jpg)
