## Informacje szczegółowe
Węzeł Extend With Arc dodaje łuk kołowy do początku lub końca wejściowej krzywej PolyCurve i zwraca jedną połączoną krzywą PolyCurve. Wartość wejściowa promienia (radius) określa promień okręgu, natomiast wartość wejściowa długości (length) określa odległość wzdłuż okręgu dla łuku. Całkowita długość musi być mniejsza niż lub równa długości całego okręgu o podanym promieniu. Wygenerowany łuk będzie styczny do końca łuku wejściowej krzywej PolyCurve. Wartość logiczna endOrStart określa, na którym końcu krzywej PolyCurve łuk zostanie utworzony. Wartość prawda (true) powoduje utworzenie łuku na końcu krzywej PolyCurve, a wartość prawda (false) — na początku. W poniższym przykładzie najpierw używamy zestawu punktów losowych i węzła PolyCurve By Points, aby wygenerować krzywą PolyCurve. Następnie za pomocą dwóch suwaków Number Slider i przełącznika logicznego ustawiamy parametry dla węzła ExtendWithArc.
___
## Plik przykładowy

![ExtendWithArc](./Autodesk.DesignScript.Geometry.PolyCurve.ExtendWithArc_img.jpg)

