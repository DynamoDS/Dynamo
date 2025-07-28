## Informacje szczegółowe
Węzeł Patch podejmuje próbę utworzenia powierzchni przy użyciu krzywej wejściowej (curve) jako obwiedni. Krzywa wejściowa musi być zamknięta. W poniższym przykładzie najpierw używamy węzła Point.ByCylindricalCoordinates, aby utworzyć zestaw punktów w ustalonych odstępach na okręgu, ale z losowymi rzędnymi i promieniami. Następnie używamy węzła NurbsCurve.ByPoints, aby utworzyć krzywą zamkniętą na podstawie tych punktów. Węzeł Patch jest używany do utworzenia powierzchni z tej krzywej zamkniętej obwiedni. Należy pamiętać, że ponieważ punkty utworzono za pomocą losowych promieni i rzędnych, nie wszystkie rozmieszczenia dają krzywą, którą można skutecznie przetworzyć za pomocą węzła Patch.
___
## Plik przykładowy

![Patch](./Autodesk.DesignScript.Geometry.Curve.Patch_img.jpg)

