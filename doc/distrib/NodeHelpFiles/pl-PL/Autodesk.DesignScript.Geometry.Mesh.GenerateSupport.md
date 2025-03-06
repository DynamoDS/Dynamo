## Informacje szczegółowe
Węzeł `Mesh.GenerateSupport` służy do dodawania podpór do wejściowej geometrii siatki w celu przygotowania jej do drukowania 3D. Do pomyślnego drukowania geometrii ze zwisami wymagane są podpory, aby zapewnić odpowiednią przyczepność warstwy i zapobiec zwisaniu materiału podczas procesu drukowania. Węzeł `Mesh.GenerateSupport` wykrywa zwisy i automatycznie generuje podpory drzewiaste, które zużywają mniej materiału i które można łatwiej usunąć, ponieważ mają mniejszy kontakt z drukowaną powierzchnią. W przypadkach, w których nie zostaną wykryte zwisy, wynikiem węzła `Mesh.GenerateSupport` jest ta sama siatka, obrócona do optymalnej orientacji na potrzeby drukowania i przekształcona w płaszczyznę XY. Konfiguracją podpór steruje się za pomocą pozycji danych wejściowych:
- baseHeight - określa grubość najniższej części podpory, jej podstawy
- baseDiameter steruje rozmiarem podstawy podpory
- postDiameter to pozycja danych wejściowych sterująca rozmiarem każdej podpory w jej środku
- tipHeight i tipDiameter sterują rozmiarem podpór na ich końcach stykających się z drukowaną powierzchnią
W poniższym przykładzie węzeł `Mesh.GenerateSupport` służy do dodawania podpór do siatki w kształcie litery „T”.

## Plik przykładowy

![Example](./Autodesk.DesignScript.Geometry.Mesh.GenerateSupport_img.jpg)
