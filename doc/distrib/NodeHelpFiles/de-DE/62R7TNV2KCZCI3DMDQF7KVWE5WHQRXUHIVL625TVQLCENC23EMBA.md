<!--- Autodesk.DesignScript.Geometry.Surface.ToNurbsSurface(surface, limitSurface) --->
<!--- 62R7TNV2KCZCI3DMDQF7KVWE5WHQRXUHIVL625TVQLCENC23EMBA --->
## Im Detail
`Surface.ToNurbsSurface` verwendet eine Oberfläche als Eingabe und gibt ein NurbsSurface-Objekt zurück, das der eingegebenen Oberfläche angenähert wird. Die `limitSurface`-Eingabe bestimmt, ob die Oberfläche vor der Konvertierung auf den ursprünglichen Parameterbereich zurückgesetzt werden soll, z. B. wenn der Parameterbereich einer Oberfläche nach einem Stutzen-Vorgang beschränkt ist.

Im folgenden Beispiel wird mithilfe eines `Surface.ByPatch`-Blocks eine Oberfläche mit einem geschlossenen NurbsCurve-Objekt als Eingabe erstellt. Beachten Sie, dass bei Verwendung dieser Oberfläche als Eingabe für einen `Surface.ToNurbsSurface`-Block ein ungestutztes NurbsSurface-Objekt mit vier Seiten entsteht.


___
## Beispieldatei

![Surface.ToNurbsSurface](./62R7TNV2KCZCI3DMDQF7KVWE5WHQRXUHIVL625TVQLCENC23EMBA_img.jpg)
