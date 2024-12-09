<!--- Autodesk.DesignScript.Geometry.TSpline.TSplineSurface.BuildFromLines --->
<!--- UZBX3TZTQ23FP32HELAYI7UOVGU7J6ACDZ5C3DTCYCIVJOHYWCCQ --->
## Informacje szczegółowe
Węzeł `TSplineSurface.BuildFromLines` umożliwia utworzenie bardziej złożonej powierzchni T-splajn, której można używać jako geometrii końcowej lub jako prymitywu niestandardowego bliższego żądanemu kształtowi niż prymitywy domyślne. Wynik może być powierzchnią zamkniętą lub otwartą oraz może mieć otwory i/lub pofałdowane krawędzie.

Dane wejściowe węzła to lista krzywych reprezentująca klatkę ochronną, `control cage`, dla powierzchni T-splajn. Skonfigurowanie tej listy linii wymaga przygotowania i musi ona być zgodna z określonymi wytycznymi.
— linie nie mogą nakładać się na siebie
— obramowanie wieloboku musi być zamknięte, a każdy punkt końcowy linii musi stykać się z co najmniej jednym innym punktem końcowym. Każde przecięcie linii musi stykać się w punkcie.
— dla obszarów o większej szczegółowości jest wymagana większa gęstość wieloboków
— preferowane są czworokąty zamiast trójkątów i N-boków, ponieważ łatwiej nimi sterować.

W poniższym przykładzie zostają utworzone dwie powierzchnie T-splajn w celu zilustrowania użycia tego węzła. Dla pozycji `maxFaceValence` zostaje w obu przypadkach pozostawiona wartość domyślna, a pozycja `snappingTolerance` zostaje dostosowana w celu zapewnienia, że linie w granicach tolerancji będą traktowane jako łączące się. W przypadku kształtu po lewej stronie dla pozycji `creaseOuterVertices` zostaje ustawiona wartość False (Fałsz), aby zachować oba wierzchołki narożników ostre i niezaokrąglone. Kształt po lewej stronie nie ma wierzchołków zewnętrznych, więc zostaje pozostawiona wartość domyślna tej pozycji danych wejściowych. Dla obu kształtów aktywowana zostaje pozycja `inSmoothMode` w celu uzyskania podglądu gładkiego.

___
## Plik przykładowy

![Example](./UZBX3TZTQ23FP32HELAYI7UOVGU7J6ACDZ5C3DTCYCIVJOHYWCCQ_img.jpg)
