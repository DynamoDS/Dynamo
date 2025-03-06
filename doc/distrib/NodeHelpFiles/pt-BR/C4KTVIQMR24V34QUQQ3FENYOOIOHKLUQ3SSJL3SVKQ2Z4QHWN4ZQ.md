<!--- Autodesk.DesignScript.Geometry.TSpline.TSplineSurface.ByNurbsSurfaceUniform --->
<!--- C4KTVIQMR24V34QUQQ3FENYOOIOHKLUQ3SSJL3SVKQ2Z4QHWN4ZQ --->
## Em profundidade
No exemplo abaixo, uma superfície NURBS de grau 3 é convertida em uma superfície da T-Spline usando o nó `TSplineSurface.ByNurbsSurfaceUniform`. A superfície NURBS de entrada é reconstruída com nós uniformes colocados em intervalos de comprimento paramétrico ou de arco iguais, dependendo das entradas `uUseArcLen` e `vUseArcLen` correspondentes, e aproximado pela superfície NURBS de grau 3. A T-Spline de saída é dividida por determinadas contagens de `uSpan` e `vSpan` nas direções U e V.

## Arquivo de exemplo

![Example](./C4KTVIQMR24V34QUQQ3FENYOOIOHKLUQ3SSJL3SVKQ2Z4QHWN4ZQ_img.jpg)
