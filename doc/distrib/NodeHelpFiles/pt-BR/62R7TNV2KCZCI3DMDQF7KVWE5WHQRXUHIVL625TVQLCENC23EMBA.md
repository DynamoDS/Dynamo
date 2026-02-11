<!--- Autodesk.DesignScript.Geometry.Surface.ToNurbsSurface(surface, limitSurface) --->
<!--- 62R7TNV2KCZCI3DMDQF7KVWE5WHQRXUHIVL625TVQLCENC23EMBA --->
## Em profundidade
`Surface.ToNurbsSurface` usa uma superfície como entrada e retorna uma NurbsSurface que se aproxima da superfície de entrada. A entrada `limitSurface` determina se a superfície deve ser restaurada para seu intervalo de parâmetros original antes da conversão, por exemplo, quando o intervalo de parâmetros de uma superfície é limitado após uma operação Aparar.

No exemplo abaixo, criamos uma superfície usando um nó `Surface.ByPatch` com uma NurbsCurve fechada como entrada. Observe que quando usamos essa superfície como entrada para um nó `Surface.ToNurbsSurface`, o resultado é uma NurbsSurface não aparada com quatro lados.


___
## Arquivo de exemplo

![Surface.ToNurbsSurface](./62R7TNV2KCZCI3DMDQF7KVWE5WHQRXUHIVL625TVQLCENC23EMBA_img.jpg)
