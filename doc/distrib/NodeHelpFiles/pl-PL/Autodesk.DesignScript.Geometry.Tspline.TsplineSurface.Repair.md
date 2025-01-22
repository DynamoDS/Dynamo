## Informacje szczegółowe
W poniższym przykładzie powierzchnia T-splajn staje się nieprawidłowa, co można stwierdzić, obserwując nakładające się powierzchnie w podglądzie w tle. Nieprawidłowość powierzchni można potwierdzić niepowodzeniem próby aktywowania trybu gładkiego przy użyciu węzła `TSplineSurface.EnableSmoothMode`. Inną wskazówką jest to, że węzeł `TSplineSurface.IsInBoxMode` zwraca wartość `true` (prawda), nawet jeśli powierzchnia początkowo aktywowała tryb gładki.

W celu naprawienia powierzchni zostaje ona przetworzona przez węzeł `TSplineSurface.Repair`. Wynikiem jest prawidłowa powierzchnia, co można potwierdzić, pomyślnie włączając tryb gładki podglądu.
___
## Plik przykładowy

![TSplineSurface.Repair](./Autodesk.DesignScript.Geometry.TSpline.TSplineSurface.Repair_img.jpg)
