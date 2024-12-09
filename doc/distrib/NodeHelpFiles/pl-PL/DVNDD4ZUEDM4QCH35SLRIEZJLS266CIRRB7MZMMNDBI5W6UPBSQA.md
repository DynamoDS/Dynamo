<!--- Autodesk.DesignScript.Geometry.TSpline.TSplineSurface.BridgeFacesToEdges --->
<!--- DVNDD4ZUEDM4QCH35SLRIEZJLS266CIRRB7MZMMNDBI5W6UPBSQA --->
## Informacje szczegółowe
Węzeł `TSplineSurface.BridgeToFacesToEdges` łączy zestawy krawędzi z zestawem powierzchni — z tej samej powierzchni lub z dwóch różnych powierzchni. Liczba krawędzi tworzących te powierzchnie musi być zgodna z liczbą krawędzi po drugiej stronie mostu lub być tej liczby wielokrotnością. Węzeł wymaga danych wejściowych opisanych poniżej. Pierwsze trzy pozycje danych wejściowych są wystarczające do wygenerowania mostu — pozostałe dane wejściowe są opcjonalne. Wynikowa powierzchnia jest elementem podrzędnym powierzchni, do której należy pierwsza grupa krawędzi.

— `TSplineSurface`: powierzchnia, dla której należy utworzyć most
— `firstGroup`: powierzchnie z wybranej powierzchni TSplineSurface
— `secondGroup`: krawędzie z tej samej wybranej powierzchni T-splajn lub z innej. Liczba krawędzi musi być zgodna z liczbą krawędzi po drugiej stronie mostu lub być jej wielokrotnością.
— `followCurves`: (opcjonalnie) krzywa, za którą ma podążać most. W przypadku braku tej pozycji danych wejściowych most podąża za linią prostą
— `frameRotations`: (opcjonalnie) liczba obrotów wyciągnięcia prostego mostu łączącego wybrane krawędzie.
— `spansCounts`: (opcjonalnie) liczba rozpiętości/segmentów wyciągnięcia prostego mostu łączącego wybrane krawędzie. Jeśli liczba rozpiętości jest zbyt mała, niektóre opcje mogą nie być dostępne, dopóki nie zostanie zwiększona.
— `cleanBorderBridges`:(opcjonalnie) usuwa mosty między mostami obramowania, aby zapobiec fałdowaniu
— `keepSubdCreases`: (opcjonalnie) zachowuje fałdowania podziału na składowe topologii wejściowej, co skutkuje pofałdowaniem początku i końca mostu
— `firstAlignVertices` (opcjonalnie) i `secondAlignVertices`: wymuszenie wyrównania między dwoma zestawami wierzchołków zamiast automatycznego wybierania połączenia par najbliższych wierzchołków.
— `flipAlignFlags`: (opcjonalnie) odwraca kierunek wierzchołków do wyrównania


W poniższym przykładzie zostają utworzone dwie płaszczyzny T-splajn i zostają zebrane zestawy krawędzi oraz powierzchni za pomocą węzłów `TSplineTopology.VertexByIndex` i `TSplineTopology.FaceByIndex`. Aby utworzyć most, te powierzchnie i krawędzie zostają przekazane jako dane wejściowe do węzła `TSplineSurface.BrideFacesToEdges` wraz z jedną z powierzchni. Powoduje to utworzenie mostu. Do mostu dodanych zostaje więcej rozpiętości przez edytowanie pozycji danych wejściowych `spansCounts`. Gdy jako wartość wejściowa `followCurves` używana jest krzywa, most podąża za kierunkiem tej krzywej. Pozycje danych `keepSubdCreases`,`frameRotations`, `firstAlignVertices` i `secondAlignVertices` ilustrują, jak można dostosować kształt mostu.

## Plik przykładowy

![BridgeFacesToEdges](./DVNDD4ZUEDM4QCH35SLRIEZJLS266CIRRB7MZMMNDBI5W6UPBSQA_img.gif)
