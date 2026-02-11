## Informacje szczegółowe
Węzeł `Solid.ByRevolve` tworzy powierzchnię przez obrócenie danej krzywej profilu wokół osi. Ta oś jest definiowana przez punkt `axisOrigin` i wektor `axisDirection`. Kąt początkowy, mierzony w stopniach, określa, gdzie rozpocząć powierzchnię, a kąt `sweepAngle` określa, jak daleko dookoła osi kontynuować powierzchnię.

W poniższym przykładzie używamy krzywej wygenerowanej za pomocą funkcji cosinus jako krzywej profilu, a dwa suwaki Number Slider sterują wartościami `startAngle` i `sweepAngle`. Wartości `axisOrigin` i `axisDirection` pozostawiono w tym przykładzie z wartościami domyślnymi — początku globalnego i globalnej osi Z.

___
## Plik przykładowy

![ByRevolve](./Autodesk.DesignScript.Geometry.Solid.ByRevolve_img.jpg)

