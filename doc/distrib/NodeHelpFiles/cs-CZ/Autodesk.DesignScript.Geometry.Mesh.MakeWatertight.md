## Podrobnosti
Uzel `Mesh.MakeWatertight` vytvoří pevnou, vodotěsnou a 3D tisknutelnou síť vzorkováním původní sítě. Poskytuje rychlý způsob řešení sítě s mnoha problémy, například protínání sebe sama, překrývání a nerozložená geometrie. Metoda vypočítá pole vzdálenosti tenkého proužku a vygeneruje novou síť pomocí algoritmu Marching cubes, neprovede však promítnutí zpět na původní síť. Jedná se o vhodnější metodu pro objekty sítě, které mají řadu vad nebo obtížné problémy, například protínání sebe sama.
Následující příklad ukazuje nevodotěsnou vázu a její vodotěsný ekvivalent.

## Vzorový soubor

![Example](./Autodesk.DesignScript.Geometry.Mesh.MakeWatertight_img.jpg)
