## Informacje szczegółowe
Węzeł `Geometry.DeserializeFromSABWithUnits` importuje geometrię do dodatku Dynamo z tablicy bajtowej .SAB (Standard ACIS Binary) i z `DynamoUnit.Unit` z możliwością przekonwertowania z milimetrów. Ten węzeł pobiera tablicę byte[] jako pierwszą pozycję wejściową i jednostkę `dynamoUnit` jako drugą. W przypadku pozostawienia pozycji danych wejściowych `dynamoUnit` o wartości null geometria .SAB zostanie zaimportowana jako niemianowana — dane geometrii w tablicy zostaną zaimportowane bez konwersji jednostek. W przypadku podania jednostki jednostki wewnętrzne tablicy .SAB zostaną przekonwertowane na określone jednostki.

Dodatek Dynamo jest niemianowany (bez jednostek), ale wartości liczbowe na wykresie Dynamo prawdopodobnie mają pewne jednostki niejawne. Za pomocą pozycji wejściowej `dynamoUnit` można przeskalować geometrię wewnętrzną pliku .SAB do tego systemu jednostek.

W poniższym przykładzie zostaje wygenerowany prostopadłościan na podstawie tablicy SAB z 2 jednostkami miary (bez jednostek). Pozycja danych wejściowych `dynamoUnit` powoduje przeskalowanie wybranej jednostki do użycia w innym oprogramowaniu.

___
## Plik przykładowy

![Geometry.DeserializeFromSABWithUnits](./GeometryUI.DeserializeFromSABWithUnits_img.jpg)
