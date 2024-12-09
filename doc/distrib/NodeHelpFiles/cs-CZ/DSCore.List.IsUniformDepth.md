## Podrobnosti
Uzel `List.IsUniformDepth` vrací booleovskou hodnotu podle toho, zda je hloubka seznamu konzistentní, což znamená, že každý seznam má stejný počet seznamů vnořených uvnitř tohoto seznamu.

V následujícím příkladu jsou porovnány dva seznamy, jeden s jednotnou hloubkou a jeden s nejednotnou hloubkou, aby se znázornil rozdíl. Jednotný seznam obsahuje tři seznamy bez vnořených seznamů. Nejednotný seznam obsahuje dva seznamy. První seznam nemá vnořené seznamy, druhý má dva vnořené seznamy. Seznamy na indexech [0] a [1] nejsou stejné hloubky, takže uzel `List.IsUniformDepth` vrátí hodnotu False.
___
## Vzorový soubor

![List.IsUniformDepth](./DSCore.List.IsUniformDepth_img.jpg)
