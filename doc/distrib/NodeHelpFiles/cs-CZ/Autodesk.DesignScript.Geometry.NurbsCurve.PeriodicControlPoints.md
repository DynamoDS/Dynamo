## Podrobnosti
Uzel `NurbsCurve.PeriodicControlPoints` použijte pokud potřebujete exportovat uzavřenou křivku NURBS do jiného systému (například do aplikace Alias) nebo pokud daný systém očekává křivku v periodické formě. Mnoho nástrojů CAD očekává tuto formu kvůli přesnosti zpracování.

Uzel `PeriodicControlPoints` vrací řídicí body v *periodické* formě. Uzel `ControlPoints` je vrátí ve *svázané* formě. Obě pole mají stejný počet bodů; jedná se o dva různé způsoby, jak popsat stejnou křivku. V periodické formě se posledních několik řídicích bodů shoduje s několika prvními (podle stupně křivky), takže se křivka uzavře plynule. Svázaná forma používá jiné rozvržení, takže pozice bodů v obou polích se liší.

V následujícím příkladu je vytvořena periodická křivka NURBS pomocí uzlu `NurbsCurve.ByControlPointsWeightsKnots`. Uzly Watch porovnají položky `ControlPoints` a `PeriodicControlPoints`, abyste viděli stejnou délku, ale různé pozice bodů. Objekty ControlPoints se zobrazují červenou barvou, takže se v náhledu na pozadí zobrazují odlišně od objektů PeriodicControlPoints, které jsou černé.
___
## Vzorový soubor

![PeriodicControlPoints](./Autodesk.DesignScript.Geometry.NurbsCurve.PeriodicControlPoints_img.jpg)
