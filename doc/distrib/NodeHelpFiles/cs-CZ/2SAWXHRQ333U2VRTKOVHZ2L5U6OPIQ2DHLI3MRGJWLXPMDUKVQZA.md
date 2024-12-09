<!--- Autodesk.DesignScript.Geometry.NurbsSurface.ByControlPointsWeightsKnots --->
<!--- 2SAWXHRQ333U2VRTKOVHZ2L5U6OPIQ2DHLI3MRGJWLXPMDUKVQZA --->
## Podrobnosti
Vytvoří objekt NurbsSurface pomocí určených řídicích vrcholů, uzlů, vah a stupňů U a V. Existuje několik omezení dat, která v případě porušení způsobí selhání funkce a vznik výjimky. Stupeň: stupně U i V by měly být >= 1 (lineární spline po částech) a menší než 26 (maximální stupeň založený na B-spline podporovaný ASM). Tloušťky čar: Všechny hodnoty vah (pokud jsou zadány) by měly být zásadně kladné. Tloušťka čáry menší než 1e-11 bude zamítnuta a funkce selže. Uzly: oba uzlové vektory by měly být nesestupné posloupnosti. Vnitřní různorodost uzlů by neměla být větší než stupeň plus 1 na počátečním nebo koncovém uzlu a stupeň na interním uzlu (toto umožňuje reprezentaci povrchů s přerušeními G1). Všimněte si, že jsou podporovány nesvázané vektory uzlů, takové však budou převedeny na svázané tím, že u nich budou provedeny odpovídající změny použité v datech řídicího bodu / váhy.
___
## Vzorový soubor



