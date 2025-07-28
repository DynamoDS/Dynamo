<!--- Autodesk.DesignScript.Geometry.TSpline.TSplineSurface.BridgeFacesToEdges --->
<!--- DVNDD4ZUEDM4QCH35SLRIEZJLS266CIRRB7MZMMNDBI5W6UPBSQA --->
## Podrobnosti
Uzel `TSplineSurface.BridgeToFacesToEdges` propojí sady hran se sadou ploch, buď ze stejného povrchu nebo ze dvou různých povrchů. Hran tvořících plochy musí být stjený počet, nebo jejich počet musí být násobkem počtu hran na druhé straně mostu. Uzel vyžaduje vstupy popsané níže. První tři vstupy jsou dostačující k vygenerování mostu, zbývající vstupy jsou volitelné. Výsledný povrch je podřazený povrchu, do kterého patří první skupina hran.

- `TSplineSurface`: povrch, který chcete přemostit
- `firstGroup`: Plochy z vybraného objektu TSplineSurface
- `secondGroup`: hrany buď ze stejného vybraného povrchu T-Spline, nebo z jiného povrchu. Počet hran musí být stejný nebo musí být násobkem počtu hran na druhé straně mostu.
- `followCurves`: (volitelné) křivka, kterou má most sledovat. Pokud tento vstup neexistuje, přemostění sleduje přímou čáru
- `frameRotations`: (volitelné) počet otočení vysunutí mostu, které spojuje vybrané hrany.
- `spansCounts`: (volitelné) počet rozpětí / segmentů vysunutí mostu, které spojuje vybrané hrany. Pokud je počet rozpětí příliš nízký, některé možnosti nemusí být k dispozici, dokud se počet nezvýší.
- `cleanBorderBridges`: (volitelné) odstraní mosty mezi hraničními mosty, aby se zabránilo vyostření
- `keepSubdCreases`: (volitelné) zachová vyostření dílčích částí vstupní topologie, což vede k vyostření počátku a konce mostu
- `firstAlignVertices`(volitelné) a `secondAlignVertices`: vynucení zarovnání mezi dvěma sadami vrcholů místo automatického výběru spojení dvojic nejbližších vrcholů.
- `flipAlignFlags`: (volitelné) obrátí směr vrcholů, které mají být zarovnány.


V níže uvedeném příkladu jsou vytvořeny dvě roviny T-Spline a jsou získány sady hran a ploch pomocí uzlů `TSplineTopology.VertexByIndex` a `TSplineTopology.FaceByIndex`. V rámci vytvoření mostu se plochy a hrany použijí jako vstup pro uzel `TSplineSurface.BrideFacesToEdges` společně s jedním z povrchů. Tím se vytvoří most. Úpravou vstupu `spansCounts` se k mostu přidá více rozpětí. Pokud se u vstupu `followCurves` použije křivka, most následuje směr zadané křivky. Vstupy `keepSubdCreases`,`frameRotations`, `firstAlignVertices` a `secondAlignVertices` demonstrují, jak je možné dále ladit tvar mostu.

## Vzorový soubor

![BridgeFacesToEdges](./DVNDD4ZUEDM4QCH35SLRIEZJLS266CIRRB7MZMMNDBI5W6UPBSQA_img.gif)
