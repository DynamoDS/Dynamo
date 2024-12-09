## In-Depth
Uzel `TSplineSurface.BevelEdges` odsadí vybranou hranu nebo skupinu hran v obou směrech podél plochy, čímž nahradí původní hranu posloupností hran tvořících kanál.

V níže uvedeném příkladu se skupina hran základního kvádru T-Spline použije jako vstup pro uzel `TSplineSurface.BevelEdges`. Příklad znázorňuje, jak následující vstupy ovlivňují výsledek:
- vstup `percentage` určuje rozložení nově vytvořených hran podél sousedních ploch, přičemž hodnoty sousedící s nulou umisťují nové hrany blíže k původní hraně a hodnoty blížící se k 1 umisťují dále.
- vstup `numberOfSegments` určuje počet nových ploch v kanálu.
- vstup `keepOnFace` definuje, zda jsou hrany zkosení umístěny v rovině původní plochy. Pokud je hodnota nastavena na hodnotu True, vstup zaoblení nemá žádný vliv.
- vstup `roundness` určuje, jak je zaoblené zkosení, a očekává hodnotu v rozsahu od 0 do 1, přičemž výsledkem je přímé zkosení u hodnoty 0 a zaoblené zkosení u hodnoty 1.

Někdy se zapne kvádrový režim, aby byl tvar snadnější na porozumění.


## Vzorový soubor

![Example](./Autodesk.DesignScript.Geometry.TSpline.TSplineSurface.BevelEdges_img.gif)
