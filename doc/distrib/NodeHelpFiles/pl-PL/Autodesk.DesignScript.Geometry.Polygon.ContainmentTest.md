## Informacje szczegółowe
Węzeł Containment Test zwraca wartość logiczną wskazującą, czy dany punkt (point) jest zawarty w danym wieloboku (polygon). Aby to zadziałało, wielobok musi być płaski i niesamoprzecinający się. W poniższym przykładzie tworzymy wielobok za pomocą serii punktów utworzonych przy użyciu węzła By Cylindrical Coordinates. Pozostawienie stałej rzędnej i sortowanie kątów zapewnia uzyskanie płaskiego i niesamoprzecinającego się wieloboku. Następnie tworzymy punkt do przetestowania i za pomocą węzła ContainmentTest sprawdzamy, czy ten punkt znajduje się wewnątrz, czy na zewnątrz wieloboku.
___
## Plik przykładowy

![ContainmentTest](./Autodesk.DesignScript.Geometry.Polygon.ContainmentTest_img.jpg)

