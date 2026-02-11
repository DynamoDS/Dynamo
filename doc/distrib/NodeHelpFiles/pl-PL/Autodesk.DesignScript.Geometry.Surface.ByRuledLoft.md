## Informacje szczegółowe
Węzeł Surface by Ruled Loft pobiera uporządkowaną listę krzywych i wyciąga między nimi powierzchnię po linii prostej. W porównaniu do węzła ByLoft węzeł ByRuledLoft może działać nieco szybciej, ale wynikowa powierzchnia jest mniej gładka. W poniższym przykładzie zaczynamy od linii wzdłuż osi X. Przekształcamy tę linię w szereg linii, które podążają za krzywą sinusoidalną w kierunku osi Y. Użycie wynikowej listy linii jako danych wejściowych dla węzła Surface ByRuledLoft powoduje utworzenie powierzchni z segmentami biegnącymi w linii prostej między krzywymi wejściowymi.
___
## Plik przykładowy

![ByRuledLoft](./Autodesk.DesignScript.Geometry.Surface.ByRuledLoft_img.jpg)

