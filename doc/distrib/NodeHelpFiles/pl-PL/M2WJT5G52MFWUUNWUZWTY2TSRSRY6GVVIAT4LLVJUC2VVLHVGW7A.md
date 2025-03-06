<!--- Autodesk.DesignScript.Geometry.TSpline.TSplineReflection.SegmentAngle --->
<!--- M2WJT5G52MFWUUNWUZWTY2TSRSRY6GVVIAT4LLVJUC2VVLHVGW7A --->
## In-Depth
Węzeł `TSplineReflection.SegmentAngle` zwraca kąt między każdą parą segmentów odbicia promieniowego. Jeśli typ odbicia TSplineReflection jest osiowy (Axial), węzeł zwraca 0.

W poniższym przykładzie zostaje utworzona powierzchnia T-splajn z dodanymi odbiciami. Na dalszym etapie wykresu powierzchnia zostaje sprawdzona za pomocą węzła `TSplineSurface.Reflections`. Wynik (odbicie) zostaje następnie przekazany jako dane wejściowe do węzła `TSplineReflection.SegmentAngle`, aby zwrócić kąt między segmentami odbicia promieniowego.

## Plik przykładowy

![Example](./M2WJT5G52MFWUUNWUZWTY2TSRSRY6GVVIAT4LLVJUC2VVLHVGW7A_img.jpg)
