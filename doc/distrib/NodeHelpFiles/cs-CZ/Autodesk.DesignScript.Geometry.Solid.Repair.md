## Podrobnosti
Uzel `Solid.Repair` se pokusí opravit tělesa, která mají nesprávnou geometrii a také se pokusí provést potenciální optimalizace. Uzel `Solid.Repair` vrací nový objekt tělesa.

Tento uzel je užitečný, pokud se setkáte s chybami při provádění operací na importované nebo převedené geometrii.

V následujícím příkladu je pomocí uzlu `Solid.Repair` opravena geometrie ze souboru **. SAT** soubor. Geometrii v souboru nelze oříznout, ani u ní provést booleovskou operaci a uzel `Solid.Repair` odstraní *neplatnou geometrii*, která způsobuje chybu.

Obecně platí, že tuto funkci není nutné používat u geometrie, kterou vytvoříte v aplikaci Dynamo, pouze u geometrie z externích zdrojů. Pokud zjistíte, že tomu tak není, nahlaste chybu týmu Dynamo na síti Githubu
___
## Vzorový soubor

![Solid.Repair](./Autodesk.DesignScript.Geometry.Solid.Repair_img.jpg)
