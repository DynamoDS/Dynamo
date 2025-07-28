## Informacje szczegółowe
Węzeł Close With Line dodaje linię prostą między punktami początkowym i końcowym otwartej krzywej PolyCurve. Zwraca nową krzywą PolyCurve zawierającą dodaną linię. W poniższym przykładzie generujemy zestaw punktów losowych i za pomocą węzła PolyCuve By Points z parametrem wejściowym connectLastToFirst ustawionym na fałsz (false) tworzymy otwartą krzywą PolyCurve. Wprowadzenie tej krzywej PolyCurve do węzła Close With Line powoduje utworzenie nowej zamkniętej krzywej PolyCurve (w tym przypadku jest to równoważne użyciu wartości wejściowej „true”, prawda, dla opcji connectLastToFirst w węźle PolyCurve By Points).
___
## Plik przykładowy

![CloseWithLine](./Autodesk.DesignScript.Geometry.PolyCurve.CloseWithLine_img.jpg)

