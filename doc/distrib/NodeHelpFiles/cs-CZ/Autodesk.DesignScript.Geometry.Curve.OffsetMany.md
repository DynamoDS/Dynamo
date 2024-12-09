## Podrobnosti
Uzel `Curve.OffsetMany` vytvoří jednu nebo více křivek odsazením rovinné křivky o zadanou vzdálenost v rovině definované normálou roviny. Pokud mezi odsazenými křivkami existují mezery, budou vyplněny prodloužením odsazených křivek.

Vstup `planeNormal` je ve výchozím nastavení normála roviny obsahující křivku, ale k lepšímu řízení směru odsazení lze použít i jinou normálu rovnoběžnou s původní normálou křivky.

Pokud je například pro více křivek sdílejících stejnou rovinu vyžadován konzistentní směr odsazení, lze pomocí vstupu `planeNormal` přepsat normály jednotlivých křivek a vynutit odsazení všech křivek ve stejném směru. Obrácením normály se obrátí směr odsazení.

V níže uvedeném příkladu je objekt polykřivky odsazen o zápornou vzdálenost odsazení, která se použije v opačném směru vektorového součinu mezi tečnou křivky a normálovým vektorem roviny.
___
## Vzorový soubor

![Curve.OffsetMany](./Autodesk.DesignScript.Geometry.Curve.OffsetMany_img.jpg)
