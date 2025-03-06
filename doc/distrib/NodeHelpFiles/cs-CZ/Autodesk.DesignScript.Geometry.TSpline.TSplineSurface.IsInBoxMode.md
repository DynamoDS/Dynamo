## Podrobnosti
Režim kvádru a režim vyhlazení jsou dva způsoby zobrazení povrchu T-Spline. Režim vyhlazení je skutečný tvar povrchu T-Spline a je užitečný pro náhled estetiky a rozměrů modelu. Režim kvádru může naopak vrhat vhled na povrchovou strukturu a usnadnit její pochopení a jedná se také o rychlejší možnost zobrazení náhledu velké nebo složité geometrie. Režimy kvádru a vyhlazení je možné ovládat v okamžiku tvorby počátečního povrchu T-Spline nebo později pomocí uzlů podobných jako `TSplineSurface.EnableSmoothMode`.

V případech, kdy se povrch T-Spline stane neplatným, se její náhled automaticky přepne do režimu kvádru. Uzel `TSplineSurface.IsInBoxMode` je dalším způsobem, jak zjistit, zda se povrch stane neplatným.

V níže uvedeném příkladu je povrch roviny T-Spline vytvořen se vstupem `smoothMode` nastaveným na hodnotu true. Dvě z jeho ploch jsou odstraněny, takže povrch není platný. Náhled povrchu se přepne do režimu kvádru, i když to ze samotného náhledu není možné poznat. Uzel `TSplineSurface.IsInBoxMode` slouží k potvrzení, že je povrch v režimu kvádru.
___
## Vzorový soubor

![TSplineSurface.IsInBoxMode](./Autodesk.DesignScript.Geometry.TSpline.TSplineSurface.IsInBoxMode_img.jpg)
