## Podrobnosti
Uzel `PolyCurve.Heal` vezme objekt PolyCurve, který protíná sám sebe, a vrací nový objekt PolyCurve, který neprotíná sám sebe. Vstupní objekt PolyCurve nemusí mít více než 3 protnutí sama sebe. Jinými slovy, pokud kterýkoli segment objektu PolyCurve protne více než 2 jiné segmenty, zacelení nebude fungovat. Zadejte hodnotu vstupu `trimLength` větší než 0 a koncové segmenty delší než hodnota `trimLength` nebudou oříznuty.

V následujícím příkladu je objekt PolyCurve, který protíná sám sebe, zacelen pomocí uzlu `PolyCurve.Heal`.
___
## Vzorový soubor

![PolyCurve.Heal](./Autodesk.DesignScript.Geometry.PolyCurve.Heal_img.jpg)
