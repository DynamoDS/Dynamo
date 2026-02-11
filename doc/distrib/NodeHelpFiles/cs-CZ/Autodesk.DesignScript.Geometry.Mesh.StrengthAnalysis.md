## Podrobnosti
 Uzel `Mesh.StrengthAnalysis` vrací seznam reprezentativních barev pro každý vrchol. Výsledek lze použít společně s uzlem `Mesh.ByMeshColor`. Pevnější oblasti sítě jsou zbarveny zeleně, zatímco slabší oblasti jsou označeny žlutou až červenou tepelnou mapou. Analýza může vést k chybně pozitivním výsledkům, pokud je síť příliš hrubá nebo nepravidelná (tj. obsahuje mnoho dlouhých tenkých trojúhelníků). Před voláním uzlu `Mesh.StrengthAnalysis` můžete zkusit vygenerovat normální síť pomocí uzlu `Mesh.Remesh`, abyste dosáhli lepších výsledků.

V následujícím příkladu se používá metoda `Mesh.StrengthAnalysis` k barevnému rozlišení konstrukční pevnosti sítě ve tvaru osnovy. Výsledkem je seznam barev odpovídající délce seznamu vrcholů sítě. Tento seznam lze použít s uzlem `Mesh.ByMeshColor` k vybarvení sítě.

## Vzorový soubor

![Example](./Autodesk.DesignScript.Geometry.Mesh.StrengthAnalysis_img.jpg)
