<!--- Autodesk.DesignScript.Geometry.Solid.BySweep(profile, path, cutEndOff) --->
<!--- X65A3XAWWVM3XWMAZHZFLL5HTXCJAGYISLC4VHRMPHEV3MBYIRXQ --->
## Informacje szczegółowe
Węzeł `Solid.BySweep` tworzy bryłę przez przeciągnięcie wejściowej krzywej o profilu zamkniętym wzdłuż określonej ścieżki.

W poniższym przykładzie jako bazowej krzywej profilu używamy prostokąta. Ścieżka zostaje utworzona za pomocą funkcji cosinus z sekwencją kątów różnicujących współrzędne x zestawu punktów. Tych punktów używamy jako danych wejściowych węzła `NurbsCurve.ByPoints`. Następnie tworzymy bryłę przez przeciągnięcie prostokąta wzdłuż utworzonej krzywej cosinusoidalnej.
___
## Plik przykładowy

![Solid.BySweep](./X65A3XAWWVM3XWMAZHZFLL5HTXCJAGYISLC4VHRMPHEV3MBYIRXQ_img.jpg)
