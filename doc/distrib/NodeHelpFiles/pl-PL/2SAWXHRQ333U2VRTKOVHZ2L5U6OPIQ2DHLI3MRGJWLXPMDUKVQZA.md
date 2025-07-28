<!--- Autodesk.DesignScript.Geometry.NurbsSurface.ByControlPointsWeightsKnots --->
<!--- 2SAWXHRQ333U2VRTKOVHZ2L5U6OPIQ2DHLI3MRGJWLXPMDUKVQZA --->
## Informacje szczegółowe
Tworzy NurbsSurface z określonymi wierzchołkami sterującymi, węzłami, szerokością oraz stopniami U i V. Istnieje szereg ograniczeń dotyczących danych, które, jeśli są uszkodzone, spowodują, że funkcja zakończy się niepowodzeniem i zostanie wygenerowany wyjątek. Stopień (degree): zarówno stopień u, jak i v powinien być >= 1 (piecewise-liniowy splajn) i mniejszy niż 26 (maksymalny stopień podstawy B-splajn obsługiwany przez ASM). Szerokości (weight): wszystkie wartości szerokości (jeśli podane) powinny być wyłącznie dodatnie. Szerokości mniejsze niż 1e-11 będą odrzucane, a działanie funkcji nie powiedzie się. Węzły (knot): obydwa wektory węzłowe powinny występować w sekwencjach niemalejących. Mnogość węzła wewnętrznego nie powinna być większa niż stopień + 1 na węźle początkowym/końcowym i stopień na węźle wewnętrznym (umożliwia to przedstawienie powierzchni z nieciągłościami G1). Należy zwrócić uwagę, że niezamknięte wektory węzłowe są obsługiwane, ale zostaną przekształcone w zamknięte, a w danych dotyczących punktów sterujących/szerokości wprowadzone zostaną stosowne zmiany.
___
## Plik przykładowy



