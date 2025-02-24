## Podrobnosti
Uzel `GeometryColor.ByMeshColor` vrací objekt GeometryColor, což je síť zbarvená podle daného seznamu barev. Tento uzel lze použít několika způsoby:

- pokud je k dispozici jedna barva, celá síť je obarvena jednou danou barvou;
- pokud se počet barev shoduje s počtem trojúhelníků, každý trojúhelník je zbarven odpovídající barvou ze seznamu;
- pokud se počet barev shoduje s počtem jedinečných vrcholů, barva každého trojúhelníku v barvě sítě interpoluje mezi hodnotami barev v každém vrcholu;
- pokud se počet barev rovná počtu nejedinečných vrcholů, barva každého trojúhelníku se interpoluje mezi hodnotami barev na celé ploše, nemusí však vytvářet přechody mezi plochami.

## Příklad

V následujícím příkladu je síť barevně rozlišena podle výšky jejích vrcholů. Nejprve se pomocí uzlu `Mesh.Vertices` získají jedinečné vrcholy sítě, které se poté analyzují, a výška každého vrcholu se získá pomocí uzlu `Point.Z`. Za druhé se pomocí uzlu `Map.RemapRange` namapují hodnoty na nový rozsah 0 až 1 poměrným měřítkem každé hodnoty. Nakonec se pomocí uzlu `Color Range` vygeneruje seznam barev odpovídajících namapovaným hodnotám. Tento seznam barev použijte jako vstup `colors` uzlu `GeometryColor.ByMeshColors`. Výsledkem je barevně rozlišená síť, kde se barva každého trojúhelníku interpoluje mezi barvami vrcholů a vytváří gradient.

## Vzorový soubor

![Example](./Modifiers.GeometryColor.ByMeshColors_img.jpg)
