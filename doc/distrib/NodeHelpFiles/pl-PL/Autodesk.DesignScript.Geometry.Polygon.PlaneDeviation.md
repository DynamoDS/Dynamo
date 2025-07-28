## Informacje szczegółowe
Węzeł Plane Deviation najpierw oblicza płaszczyznę najlepszego dopasowania przechodzącą przez punkty danego wieloboku (polygon). Następnie oblicza odległość poszczególnych punktów od tej płaszczyzny, aby znaleźć maksymalne odchylenie punktów od płaszczyzny najlepszego dopasowania. W poniższym przykładzie generujemy listę losowych kątów, rzędnych i promieni, a następnie za pomocą węzła Points By Cylindrical Coordinates tworzymy zestaw punktów niepłaskich, które zostaną użyte w węźle Polygon By Points. Wprowadzając ten wielobok do węzła PlaneDeviation, możemy znaleźć średnie odchylenie punktów od płaszczyzny najlepszego dopasowania.
___
## Plik przykładowy

![PlaneDeviation](./Autodesk.DesignScript.Geometry.Polygon.PlaneDeviation_img.jpg)

