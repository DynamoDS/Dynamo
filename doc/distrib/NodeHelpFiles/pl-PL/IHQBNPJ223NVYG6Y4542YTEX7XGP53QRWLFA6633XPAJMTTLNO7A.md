<!--- Autodesk.DesignScript.Geometry.Surface.TrimWithEdgeLoops(surface, loops, tolerance) --->
<!--- IHQBNPJ223NVYG6Y4542YTEX7XGP53QRWLFA6633XPAJMTTLNO7A --->
## Informacje szczegółowe
Węzeł `Surface.TrimWithEdgeLoops`ucina powierzchnię za pomocą kolekcji jednej lub więcej zamkniętych krzywych PolyCurve, które muszą leżeć na powierzchni w określonej tolerancji. Jeśli trzeba uciąć jeden lub więcej otworów z powierzchni wejściowej, musi istnieć jedna pętla zewnętrzna określona dla obwiedni powierzchni i jedna pętla wewnętrzna dla każdego otworu. Jeśli ma zostać ucięty obszar między obwiednią powierzchni a otworami, należy zapewnić tylko pętle dla poszczególnych otworów. W przypadku powierzchni okresowej bez pętli zewnętrznej, takiej jak powierzchnia sferyczna, można sterować ucinanym obszarem przez odwrócenie kierunku krzywej pętli.

Podana tolerancja jest używana podczas określania, czy końce krzywej pokrywają się oraz czy krzywa i powierzchnia pokrywają się. Podana tolerancja nie może być mniejsza niż żadna z tolerancji używanych podczas tworzenia wejściowych krzywych PolyCurve. Wartość domyślna 0,0 oznacza, że zostanie użyta największa tolerancja używana podczas tworzenia wejściowych krzywych PolyCurve.

W poniższym przykładzie z powierzchni zostają ucięte dwie pętle i zostają zwrócone dwie nowe powierzchnie wyróżnione na niebiesko. Suwak Number Slider dostosowuje kształt nowych powierzchni.

___
## Plik przykładowy

![Surface.TrimWithEdgeLoops](./IHQBNPJ223NVYG6Y4542YTEX7XGP53QRWLFA6633XPAJMTTLNO7A_img.jpg)
