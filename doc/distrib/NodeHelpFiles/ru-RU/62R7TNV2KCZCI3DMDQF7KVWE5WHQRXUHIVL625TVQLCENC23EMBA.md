<!--- Autodesk.DesignScript.Geometry.Surface.ToNurbsSurface(surface, limitSurface) --->
<!--- 62R7TNV2KCZCI3DMDQF7KVWE5WHQRXUHIVL625TVQLCENC23EMBA --->
## Подробности
`Surface.ToNurbsSurface` принимает поверхность и возвращает объект NurbsSurface, который является аппроксимацией входной поверхности. Входной параметр `limitSurface` указывает, следует ли восстановить исходный диапазон параметров поверхности до преобразования, например, когда диапазон параметров поверхности ограничен после операции обрезки.

В примере ниже создается поверхность с помощью узла `Surface.ByPatch` с замкнутым объектом NurbsCurve в качестве входного параметра. Обратите внимание, что при использовании этой поверхности в качестве входного параметра для узла `Surface.ToNurbsSurface` в результате получается необрезанный объект NurbsSurface с четырьмя сторонами.


___
## Файл примера

![Surface.ToNurbsSurface](./62R7TNV2KCZCI3DMDQF7KVWE5WHQRXUHIVL625TVQLCENC23EMBA_img.jpg)
