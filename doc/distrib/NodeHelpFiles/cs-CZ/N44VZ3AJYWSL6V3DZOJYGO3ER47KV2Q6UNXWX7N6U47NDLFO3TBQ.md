<!--- Autodesk.DesignScript.Geometry.TSpline.TSplineVertex.FunctionalValence --->
<!--- N44VZ3AJYWSL6V3DZOJYGO3ER47KV2Q6UNXWX7N6U47NDLFO3TBQ --->
## In-Depth
Funkční valence vrcholu znamená více než jen jednoduchý počet přilehlých hran a zohledňuje virtuální čáry osnovy, které ovlivňují splynutí vrcholu v oblasti kolem něj. Nabízí přesnější představu o tom, jak vrcholy a jejich hrany ovlivňují povrch během operací deformace a zjemnění.
Pokud se uzel `TSplineVertex.FunctionalValence` použije na běžné vrcholy a body T, vrací hodnotu „4“, což znamená, že je povrch veden křivkami spline ve tvaru osnovy. Funkční valence o jakékoli jiné hodnotě než „4“znamená, že vrchol je cíp hvězdy a že přechod kolem vrcholu bude méně hladký.

V níže uvedeném příkladu se uzel `TSplineVertex.FunctionalValence` použije na dva vrcholy bodů T roviny T-Spline. Uzel `TSplineVertex.Valence` vrací hodnotu 3, zatímco funkční valence vybraných vrcholů má hodnotu 4, což je pro body T specifické. Uzly `TSplineVertex.UVNFrame` a `TSplineUVNFrame.Position` se používají k vizualizaci pozice analyzovaných vrcholů.

## Vzorový soubor

![Example](./N44VZ3AJYWSL6V3DZOJYGO3ER47KV2Q6UNXWX7N6U47NDLFO3TBQ_img.jpg)
