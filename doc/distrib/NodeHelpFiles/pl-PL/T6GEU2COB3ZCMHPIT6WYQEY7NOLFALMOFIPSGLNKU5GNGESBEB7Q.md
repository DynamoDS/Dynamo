<!--- Autodesk.DesignScript.Geometry.NurbsCurve.ByControlPointsWeightsKnots --->
<!--- T6GEU2COB3ZCMHPIT6WYQEY7NOLFALMOFIPSGLNKU5GNGESBEB7Q --->
## Informacje szczegółowe
Węzeł `NurbsCurve.ByControlPointsWeightsKnots` umożliwia ręczne sterowanie wagami i węzłami krzywej NurbsCurve. Lista wag powinna mieć taką samą długość jak lista punktów kontrolnych. Rozmiar listy węzłów musi być równy liczbie punktów kontrolnych plus stopień plus 1.

W poniższym przykładzie najpierw tworzymy krzywą NurbsCurve przez interpolację serii punktów losowych. Za pomocą węzłów, wag i punktów kontrolnych znajdujemy odpowiednie części tej krzywej. Możemy użyć węzła `List.ReplaceItemAtIndex`, aby zmodyfikować listę wag. Na koniec za pomocą węzła `NurbsCurve.ByControlPointsWeightsKnots` ponownie tworzymy krzywą NurbsCurve ze zmodyfikowanymi wagami.

___
## Plik przykładowy

![ByControlPointsWeightsKnots](./T6GEU2COB3ZCMHPIT6WYQEY7NOLFALMOFIPSGLNKU5GNGESBEB7Q_img.jpg)

