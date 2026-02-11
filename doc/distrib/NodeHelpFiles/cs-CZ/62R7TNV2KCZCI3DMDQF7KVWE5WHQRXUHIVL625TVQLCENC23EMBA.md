<!--- Autodesk.DesignScript.Geometry.Surface.ToNurbsSurface(surface, limitSurface) --->
<!--- 62R7TNV2KCZCI3DMDQF7KVWE5WHQRXUHIVL625TVQLCENC23EMBA --->
## Podrobnosti
Položka `Surface.ToNurbsSurface` přijímá povrch jako vstup a vrací objekt NurbsSurface, který aproximuje vstupní povrch. Vstup `limitSurface` určuje, zda má být povrch před převodem obnoven do původního rozsahu parametrů, například pokud je rozsah parametrů povrchu omezen po operaci oříznutí.

V následujícím příkladu vytvoříme povrch pomocí uzlu `Surface.ByPatch` s uzavřeným objektem NurbsCurve jako vstupem. Všimněte si, že pokud použijeme tento povrch jako vstup pro uzel `Surface.ToNurbsSurface`, výsledkem bude neoříznutý objekt NurbsSurface se čtyřmi stranami.


___
## Vzorový soubor

![Surface.ToNurbsSurface](./62R7TNV2KCZCI3DMDQF7KVWE5WHQRXUHIVL625TVQLCENC23EMBA_img.jpg)
