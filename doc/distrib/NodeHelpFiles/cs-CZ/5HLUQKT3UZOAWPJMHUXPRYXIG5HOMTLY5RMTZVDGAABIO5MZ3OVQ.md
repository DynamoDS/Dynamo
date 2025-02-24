<!--- Autodesk.DesignScript.Geometry.Surface.Thicken(surface, thickness, both_sides) --->
<!--- 5HLUQKT3UZOAWPJMHUXPRYXIG5HOMTLY5RMTZVDGAABIO5MZ3OVQ --->
## Podrobnosti
Uzel `Surface.Thicken (surface, thickness, both_sides)` vytvoří těleso odsazením povrchu podle vstupu `thickness` a uzavřením konců k uzavření tělesa. Tento uzel má další vstup, který určuje, zda má být těleso zesíleno na obou stranách. Vstup `both_sides` přijímá booleovskou hodnotu: True pro zesílení na obou stranách a False pro zesílení na jedné straně. Vezměte na vědomí, že parametr `thickness` určuje celkovou tloušťku konečného tělesa, takže pokud je parametr `both_sides` nastaven na hodnotu True, bude výsledek odsazen od původního povrchu o polovinu vstupní tloušťky na obou stranách.

V následujícím příkladu nejprve vytvoříme povrch pomocí uzlu `Surface.BySweep2Rails`. Poté pomocí posuvníku vytvoříme těleso, které určí vstup `thickness` uzlu `Surface.Thicken`. Booleovský přepínač určuje, zda se zesílení provede na obou stranách nebo jen na jedné.

___
## Vzorový soubor

![Surface.Thicken](./5HLUQKT3UZOAWPJMHUXPRYXIG5HOMTLY5RMTZVDGAABIO5MZ3OVQ_img.jpg)
