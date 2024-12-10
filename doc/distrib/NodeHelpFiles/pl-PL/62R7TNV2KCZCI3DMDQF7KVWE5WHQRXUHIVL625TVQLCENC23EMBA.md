<!--- Autodesk.DesignScript.Geometry.Surface.ToNurbsSurface(surface, limitSurface) --->
<!--- 62R7TNV2KCZCI3DMDQF7KVWE5WHQRXUHIVL625TVQLCENC23EMBA --->
## Informacje szczegółowe
Węzeł `Surface.ToNurbsSurface` pobiera jako dane wejściowe powierzchnię i zwraca powierzchnię NurbsSurface stanowiącą przybliżenie powierzchni wejściowej. Pozycja danych wejściowych `limitSurface` określa, czy powierzchnia powinna zostać przywrócona w pierwotnym zakresie parametrów przed konwersją, na przykład gdy zakres parametrów powierzchni jest ograniczony po operacji ucinania.

W poniższym przykładzie tworzymy powierzchnię za pomocą węzła `Surface.ByPatch`, używając jako danych wejściowych zamkniętej krzywej NurbsCurve. W przypadku użycia tej powierzchni jako danych wejściowych węzła `Surface.ToNurbsSurface` wynikiem będzie nieucięta powierzchnia NurbsSurface z czterema stronami.


___
## Plik przykładowy

![Surface.ToNurbsSurface](./62R7TNV2KCZCI3DMDQF7KVWE5WHQRXUHIVL625TVQLCENC23EMBA_img.jpg)
