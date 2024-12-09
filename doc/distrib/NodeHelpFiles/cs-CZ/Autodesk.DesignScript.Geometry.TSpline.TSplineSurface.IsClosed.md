## Podrobnosti
Uzavřený povrch je takový, který tvoří úplný tvar bez otvorů nebo hranic.
V níže uvedeném příkladu je koule T-Spline vygenerovaná uzlem `TSplineSurface.BySphereCenterPointRadius` pomocí uzlu `TSplineSurface.IsClosed` zkontrolována, zda je otevřená, což vrátí záporný výsledek. Je tomu tak proto, že koule T-Spline, přestože vypadají jako uzavřené, jsou ve skutečnosti otevřené na pólech, kde se více hran a vrcholů nachází v jednom bodě.

Mezery v kouli T-Spline se poté vyplní pomocí uzlu `TSplineSurface.FillHole`, což vede k mírné deformaci v místě, kde byl povrch vyplněn. Pokud je povrch znovu zkontrolován uzlem `TSplineSurface.IsClosed`, uzel nyní vrátí kladný výsledek, což znamená, že je tvar uzavřen.
___
## Vzorový soubor

![TSplineSurface.IsClosed](./Autodesk.DesignScript.Geometry.TSpline.TSplineSurface.IsClosed_img.jpg)
