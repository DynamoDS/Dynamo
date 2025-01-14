## Informacje szczegółowe
Węzeł `PolySurface.BySweep (rail, crossSection)` zwraca powierzchnię PolySurface przez przeciągnięcie listy połączonych, nieprzecinających się linii wzdłuż toru. Pozycja danych wejściowych `crossSection` może pobrać listę połączonych krzywych, które muszą stykać się w punkcie początkowym lub końcowym — w przeciwnym razie węzeł nie zwróci powierzchni PolySurface. Ten węzeł jest podobny do węzła `PolySurface.BySweep (rail, profile)` — jedyną różnicą jest to, że pozycja danych wejściowych `crossSection` pobiera listę krzywych, podczas gdy pozycja `profile` pobiera tylko jedną krzywą.

W poniższym przykładzie zostaje utworzona powierzchnia PolySurface przez przeciągnięcie wzdłuż łuku.


___
## Plik przykładowy

![PolySurface.BySweep](./Autodesk.DesignScript.Geometry.PolySurface.BySweep_img.jpg)
