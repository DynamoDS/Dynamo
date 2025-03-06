## Podrobnosti
Uzel Fillet vrátí nové těleso se zaoblenými hranami. Vstup edges určuje, které hrany mají být zaobleny, zatímco vstup offset určuje poloměr zaoblení. V níže uvedeném příkladu začneme krychlí s výchozími vstupy. Pokud potřebujeme získat vhodné hrany krychle, nejprve je třeba rozložit krychli a získat tak plochy jako seznam povrchů. Poté pomocí uzlu Face.Edges extrahujte hrany z krychle. Extrahujeme první hranu každé plochy pomocí uzlu GetItemAtIndex. Číselný posuvník řídí poloměr každého zaoblení.
___
## Vzorový soubor

![Fillet](./Autodesk.DesignScript.Geometry.Solid.Fillet_img.jpg)

