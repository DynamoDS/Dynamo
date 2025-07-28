## Podrobnosti
Uzel `Geometry.ImportFromSATWithUnits` importuje geometrii do aplikace Dynamo ze souboru .SAT a uzlu `DynamoUnit.Unit`, který je možné převést z milimetrů. Tento uzel přijímá jako první vstup cestu k souboru a jako druhý vstup `dynamoUnit`. Pokud je vstup `dynamoUnit` ponechán s hodnotou null, geometrie .SAT se importuje jako geometrie bez jednotek, čímž se importují geometrická data v souboru bez převodu jednotek. Pokud je zadána jednotka, vnitřní jednotky souboru .SAT budou převedeny na určené jednotky.

Aplikace Dynamo je bez jednotek, číselné hodnoty však mají v grafu aplikace Dynamo pravděpodobně i nadále nějaké implicitní jednotky. Pomocí vstupu `dynamoUnit` můžete změnit měřítko vnitřní geometrie souboru .SAT podle daného systému jednotek.

V následujícím příkladu je geometrie importována ze souboru .SAT, přičemž jednotky jsou stopy. Chcete-li, aby tento vzorový soubor fungoval ve vašem počítači, stáhněte si tento vzorový soubor SAT a nasměrujte na soubor invalid.sat uzel `File Path`.

___
## Vzorový soubor

![Geometry.ImportFromSATWithUnits](./GeometryUI.ImportFromSATWithUnits_img.jpg)
