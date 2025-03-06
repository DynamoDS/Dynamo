## Podrobnosti
Uzel `Geometry.DeserializeFromSABWithUnits` importuje geometrii do aplikace Dynamo z bajtového pole .SAB (standardní binární soubor ACIS) a uzlu `DynamoUnit.Unit`, který je možné převést z milimetrů. Tento uzel přijímá jako první vstup byte[] a jako druhý vstup `dynamoUnit`. Pokud je vstup `dynamoUnit` ponechán s hodnotou null, geometrie .SAB se importuje jako geometrie bez jednotek, čímž se importují geometrická data v poli bez převodu jednotek. Pokud je zadána jednotka, vnitřní jednotky pole .SAB budou převedeny na určené jednotky.

Aplikace Dynamo je bez jednotek, číselné hodnoty však mají v grafu aplikace Dynamo pravděpodobně i nadále nějaké implicitní jednotky. Pomocí vstupu `dynamoUnit` můžete změnit měřítko vnitřní geometrie souboru .SAB podle daného systému jednotek.

V následujícím příkladu je vygenerován kvádr ze souboru SAB se 2 měrnými jednotkami (bez jednotek). Vstup `dynamoUnit` změní měřítko vybrané jednotky k použití v jiném softwaru.

___
## Vzorový soubor

![Geometry.DeserializeFromSABWithUnits](./GeometryUI.DeserializeFromSABWithUnits_img.jpg)
