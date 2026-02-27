<!--- Autodesk.DesignScript.Geometry.Curve.SweepAsSurface(curve, path, cutEndOff) --->
<!--- DUHOUAQLX67Z6VGX2F6TGNPE2PGYDN7VGCOK6UW3D5GYILRXG3KA --->
## Em profundidade
`Curve.SweepAsSurface` criará uma superfície varrendo uma curva de entrada ao longo de um caminho especificado. No exemplo abaixo, criamos uma curva para varrer usando um bloco de código para criar três pontos de um nó `Arc.ByThreePoints`. Uma curva de caminho é criada como uma linha simples ao longo do eixo x. `Curve.SweepAsSurface` move a curva de perfil ao longo da curva do caminho criando uma superfície. O parâmetro `cutEndOff` é um booleano que controla o tratamento das extremidades da superfície varrida. Quando definido como `true`, as extremidades da superfície são cortadas perpendicularmente (normal) à curva do caminho, produzindo terminações limpas e planas. Quando definido como `false` (o padrão), as extremidades da superfície seguem a forma natural da curva de perfil sem ser aparadas, o que pode resultar em extremidades angulares ou irregulares dependendo da curvatura do caminho.
___
## Arquivo de exemplo

![Example](./DUHOUAQLX67Z6VGX2F6TGNPE2PGYDN7VGCOK6UW3D5GYILRXG3KA_img.jpg)

