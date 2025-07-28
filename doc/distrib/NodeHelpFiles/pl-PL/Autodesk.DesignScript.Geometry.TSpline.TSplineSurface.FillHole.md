## Informacje szczegółowe
W poniższym przykładzie wypełnione zostają przerwy w powierzchni walca T-splajn za pomocą węzła `TSplineSurface.FillHole`, co wymaga następujących danych wejściowych:
— `edges`: liczba krawędzi obramowania wybranych z powierzchni T-splajn do wypełnienia
— `fillMethod`: wartość liczbowa od 0 do 3 wskazująca metodę wypełniania:
    * 0 powoduje wypełnienie otworu mozaiką
    * 1 powoduje wypełnienie otworu pojedynczą powierzchnią N-bok
    * 2 powoduje utworzenie punktu w środku otworu, od którego powierzchnie trójkątne rozchodzą się w kierunku krawędzi
    * 3 daje podobny efekt do metody 2 z tą różnicą, że wierzchołki środkowe zostają złączone w jeden wierzchołek, a nie ustawione jeden na drugim.
— `keepSubdCreases`: wartość logiczna (boolean) wskazująca, czy fałdowania podrzędne są zachowywane.
___
## Plik przykładowy

![TSplineSurface.FillHole](./Autodesk.DesignScript.Geometry.TSpline.TSplineSurface.FillHole_img.gif)
