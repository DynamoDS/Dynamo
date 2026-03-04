## Podrobnosti
Uzel `NurbsCurve.PeriodicKnots`použijte, pokud potřebujete exportovat uzavřenou křivku NURBS do jiného systému (například do aplikace Alias) nebo pokud daný systém očekává křivku v periodické podobě. Mnoho nástrojů CAD očekává tuto formu kvůli přesnosti zpracování.

Uzel `PeriodicKnots` vrací vektor uzlu v *periodické* (nesvázané) formě. Uzel `Knots` jej vrátí ve *svázané* formě. Obě pole mají stejnou délku; jedná se o dva různé způsoby, jak popsat stejnou křivku. Ve svázané formě se uzly na začátku a na konci opakují, takže je křivka zamknutá v rozsahu parametru. V periodické formě se místo toho na začátku a na konci opakuje rozteč uzlů, což vytváří hladkou uzavřenou smyčku.

V následujícím příkladu je vytvořena periodická křivka NURBS pomocí uzlu `NurbsCurve.ByControlPointsWeightsKnots`. Uzly Watch porovnávají uzly `Knots` a `PeriodicKnots`, takže můžete vidět stejnou délku, ale různé hodnoty. Knots je svázaná forma (s opakovanými uzly na koncích) a PeriodicKnots je nesvázaná forma s opakujícím se rozdílovým vzorem, který definuje periodicitu křivky.
___
## Vzorový soubor

![PeriodicKnots](./Autodesk.DesignScript.Geometry.NurbsCurve.PeriodicKnots_img.jpg)
