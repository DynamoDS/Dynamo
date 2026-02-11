## Informacje szczegółowe
Węzeł Split By Points dzieli krzywą wejściową (curve) w określonych punktach (points) i zwraca listę wynikowych segmentów. Jeśli określone punkty nie znajdują się na krzywej, węzeł znajdzie punkty wzdłuż krzywej, które są najbliższe punktom wejściowym i podzieli krzywą w tych punktach wynikowych. W poniższym przykładzie najpierw tworzymy krzywą Nurbs za pomocą węzła ByPoints na podstawie zestawu losowo wygenerowanych punktów. Ten sam zestaw punktów to używany jako lista punktów w węźle SplitByPoints. Wynikiem jest lista segmentów krzywej między wygenerowanymi punktami.
___
## Plik przykładowy

![SplitByPoints](./Autodesk.DesignScript.Geometry.Curve.SplitByPoints_img.jpg)

