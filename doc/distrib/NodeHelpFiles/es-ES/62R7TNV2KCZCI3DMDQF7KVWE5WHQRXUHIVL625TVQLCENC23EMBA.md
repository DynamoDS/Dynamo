<!--- Autodesk.DesignScript.Geometry.Surface.ToNurbsSurface(surface, limitSurface) --->
<!--- 62R7TNV2KCZCI3DMDQF7KVWE5WHQRXUHIVL625TVQLCENC23EMBA --->
## En detalle:
`Surface.ToNurbsSurface` utiliza una superficie como entrada y devuelve una NurbsSurface que se aproxima a la superficie de entrada. La entrada `limitSurface` determina si la superficie debe restaurarse a su rango de parámetros original antes de la conversión, por ejemplo, cuando el rango de parámetros de una superficie está limitado después de una operación de recorte.

En el ejemplo siguiente, se crea una superficie mediante un nodo `Surface.ByPatch` con una NurbsCurve cerrada como entrada. Tenga en cuenta que cuando se utiliza esta superficie como entrada para el nodo `Surface.ToNurbsSurface`, el resultado es una NurbsSurface sin recortar con cuatro lados.


___
## Archivo de ejemplo

![Surface.ToNurbsSurface](./62R7TNV2KCZCI3DMDQF7KVWE5WHQRXUHIVL625TVQLCENC23EMBA_img.jpg)
