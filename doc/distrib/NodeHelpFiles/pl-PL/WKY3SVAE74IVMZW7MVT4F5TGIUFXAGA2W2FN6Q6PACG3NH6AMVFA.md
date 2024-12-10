<!--- Autodesk.DesignScript.Geometry.TSpline.TSplineSurface.SubdivideFaces --->
<!--- WKY3SVAE74IVMZW7MVT4F5TGIUFXAGA2W2FN6Q6PACG3NH6AMVFA --->
## Informacje szczegółowe
W poniższym przykładzie zostaje wygenerowana powierzchnia T-splajn za pomocą węzła `TSplineSurface.ByBoxLengths`.
Za pomocą węzła `TSplineTopology.FaceByIndex` zostaje wybrana powierzchnia i zostaje ona podzielona na składowe przy użyciu węzła `TSplineSurface.SubdivideFaces`.
Ten węzeł dzieli określone powierzchnie na mniejsze powierzchnie — cztery w przypadku zwykłych powierzchni albo trzy, pięć lub więcej w przypadku N-boków.
Gdy wartość logiczna (Boolean) pozycji danych wejściowych `exact` jest ustawiona na true (prawda), wynikiem jest powierzchnia, która próbuje zachować dokładnie taki sam kształt jak pierwotna przy dodaniu podziału na składowe. W celu zachowania kształtu dodanych może zostać więcej krzywych izometrycznych. W przypadku ustawienia wartości false (fałsz), węzeł tylko dzieli na składowe tę wybraną powierzchnię, co często powoduje powstanie powierzchni różniącej się od pierwotnej.
Za pomocą węzłów `TSplineFace.UVNFrame` i `TSplineUVNFrame.Position` zostaje wyróżniony środek powierzchni dzielonej na składowe.
___
## Plik przykładowy

![TSplineSurface.SubdivideFaces](./WKY3SVAE74IVMZW7MVT4F5TGIUFXAGA2W2FN6Q6PACG3NH6AMVFA_img.jpg)
