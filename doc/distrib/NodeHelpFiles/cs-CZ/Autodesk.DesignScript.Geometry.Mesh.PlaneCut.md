## Podrobnosti
Uzel `Mesh.PlaneCut` vrací síť, která byla oříznuta danou rovinou. Výsledkem řezu je část sítě, která leží na straně roviny ve směru normály vstupní roviny `plane`. Parametr `makeSolid` určuje, zda je se sítí zacházeno jako s tělesem `Solid`, v takovém případě je řez vyplněn nejmenším možným počtem trojúhelníků, aby byla pokryta každá díra.

V následujícím příkladu je dutá síť získaná operací `Mesh.BooleanDifference` oříznuta rovinou pod úhlem.

## Vzorový soubor

![Example](./Autodesk.DesignScript.Geometry.Mesh.PlaneCut_img.jpg)
