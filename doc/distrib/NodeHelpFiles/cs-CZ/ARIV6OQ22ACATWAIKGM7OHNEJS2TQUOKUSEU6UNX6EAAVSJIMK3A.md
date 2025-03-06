<!--- Autodesk.DesignScript.Geometry.TSpline.TSplineSurface.CompressIndexes --->
<!--- ARIV6OQ22ACATWAIKGM7OHNEJS2TQUOKUSEU6UNX6EAAVSJIMK3A --->
## Podrobnosti
Uzel `TSplineSurface.CompressIndexes` odebere mezery v indexových číslech hran, vrcholů nebo ploch povrchu T-spline, které vznikají při různých operacích, například při odstranění plochy. Pořadí indexů je zachováno.

V níže uvedeném příkladu je z povrchu základní čtyřúhelníkové koule odstraněno několik ploch, což ovlivní indexy hran tvaru. Pomocí uzlu`TSplineSurface.CompressIndexes` se opraví indexy hran tvaru, a bude tedy poté možné vybrat hranu s indexem 1.

## Vzorový soubor

![Example](./ARIV6OQ22ACATWAIKGM7OHNEJS2TQUOKUSEU6UNX6EAAVSJIMK3A_img.jpg)
