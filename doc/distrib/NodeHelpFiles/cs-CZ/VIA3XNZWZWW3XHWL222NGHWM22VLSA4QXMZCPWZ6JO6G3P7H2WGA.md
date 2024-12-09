<!--- Autodesk.DesignScript.Geometry.TSpline.TSplineTopology.DecomposedFaces --->
<!--- VIA3XNZWZWW3XHWL222NGHWM22VLSA4QXMZCPWZ6JO6G3P7H2WGA --->
## Podrobnosti
V níže uvedeném příkladu je rovinný povrch T-Spline s vysunutými, dále rozdělenými a taženými vrcholy a plochami zkontrolován pomocí uzlu `TSplineTopology.DecomposedFaces`, který vrací seznam následujících typů ploch obsažených v povrchu TSpline:

- `all`: seznam všech ploch
- `regular`: seznam běžných ploch
- `nGons`: seznam ploch NGon
- `border`: seznam okrajových ploch
- `inner`: seznam vnitřních ploch

Uzly `TSplineFace.UVNFrame` a `TSplineUVNFrame.Position` slouží ke zvýraznění různých typů ploch povrchu.
___
## Vzorový soubor

![TSplineTopology.DecomposedFaces](./VIA3XNZWZWW3XHWL222NGHWM22VLSA4QXMZCPWZ6JO6G3P7H2WGA_img.gif)
