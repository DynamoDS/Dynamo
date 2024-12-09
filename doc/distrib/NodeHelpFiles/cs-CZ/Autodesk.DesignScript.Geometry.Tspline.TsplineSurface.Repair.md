## Podrobnosti
V níže uvedeném příkladu se povrch T-Spline stane neplatným, což je možné zpozorovat tak, že v náhledu pozadí uvidíte překrývající se plochy. Skutečnost, že je povrch neplatný, je možné potvrdit selháním při aktivaci režimu vyhlazení pomocí uzlu `TSplineSurface.EnableSmoothMode`. Dalším projevem může být, že uzel `TSplineSurface.IsInBoxMode` vrací hodnotu `true`, a to i v případě, že byl povrch původně aktivován v režimu vyhlazení.

Povrch bude opraven, pokud bude předán do uzlu `TSplineSurface.Repair`. Výsledkem je platný povrch, jehož platnost je možné potvrdit úspěšným povolením režimu vyhlazení náhledu.
___
## Vzorový soubor

![TSplineSurface.Repair](./Autodesk.DesignScript.Geometry.TSpline.TSplineSurface.Repair_img.jpg)
