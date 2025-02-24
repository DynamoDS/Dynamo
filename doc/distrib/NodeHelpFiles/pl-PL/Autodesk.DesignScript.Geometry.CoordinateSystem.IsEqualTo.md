## Informacje szczegółowe
Węzeł IsEqualTo zwraca wartość logiczną zależną od tego, czy wartości wejściowych układów współrzędnych są równe. W poniższym przykładzie do węzła IsEqualTo przekazano dwa układy współrzędnych z identycznymi dodatnimi i ujemnymi położeniami punktów początkowych, co powoduje zwrócenie wartości logicznej fałsz (false). Jednak odwrócenie dodatniej wartości CoordinateSystem powoduje zwrócenie przez węzeł IsEqualTo wartości logicznej prawda (true), ponieważ wartości X i Y jego punktu początkowego są teraz ujemne.
___
## Plik przykładowy

![IsEqualTo](./Autodesk.DesignScript.Geometry.CoordinateSystem.IsEqualTo_img.jpg)

