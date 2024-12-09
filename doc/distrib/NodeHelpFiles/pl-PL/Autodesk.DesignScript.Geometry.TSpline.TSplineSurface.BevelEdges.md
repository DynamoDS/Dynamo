## In-Depth
Węzeł `TSplineSurface.BevelEdges` odsuwa wybraną krawędź lub grupę krawędzi w obu kierunkach wzdłuż powierzchni, zastępując krawędź pierwotną sekwencją krawędzi tworzących kanał.

W poniższym przykładzie grupa krawędzi prymitywu prostopadłościanowego T-splajn jest używana jako dane wejściowe węzła `TSplineSurface.BevelEdges`. Ten przykład ilustruje, jak następujące pozycje danych wejściowych wpływają na wynik:
— `percentage`: steruje rozkładem nowo utworzonych krawędzi wzdłuż sąsiednich powierzchni. Wartości bliskie zeru powodują umieszczenie nowych krawędzi bliżej krawędzi pierwotnej, a wartości bliskie 1 — umieszczenie ich dalej.
— `numberOfSegments`: steruje liczbą nowych powierzchni w kanale.
— `keepOnFace`: określa, czy krawędzie skośne są umieszczane na płaszczyźnie powierzchni pierwotnej. W przypadku ustawienia wartości True (Prawda) pozycja danych wejściowych zaokrąglenia nie ma żadnego wpływu.
— `roundness`: steruje poziomem zaokrąglenia skosu. Oczekiwana jest wartość z zakresu od 0 do 1, przy czym 0 powoduje powstanie skosu prostego, a 1 — skosu zaokrąglonego.

Czasami włączony zostaje tryb ramki, aby ułatwić interpretację kształtu.


## Plik przykładowy

![Example](./Autodesk.DesignScript.Geometry.TSpline.TSplineSurface.BevelEdges_img.gif)
