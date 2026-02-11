<!--- Autodesk.DesignScript.Geometry.Curve.SweepAsSolid(curve, path, cutEndOff) --->
<!--- LUVHU25JWECNEBKIBZFH6N5EUAM42XM3BSEOTMCI3TQDNS5EKLXA --->
## Em profundidade
`Curve.SweepAsSolid` cria um sólido varrendo uma curva de perfil fechado de entrada ao longo de um caminho especificado.

No exemplo abaixo, usamos um retângulo como curva do perfil base. O caminho é criado usando uma função de cosseno com uma sequência de ângulos para variar as coordenadas x de um conjunto de pontos. Os pontos são usados como entrada para um nó `NurbsCurve.ByPoints`. Em seguida, criamos um sólido varrendo o retângulo ao longo da curva de cosseno criada com um nó `Curve.SweepAsSolid`.
___
## Arquivo de exemplo

![Curve.SweepAsSolid(curve, path, cutEndOff)](./LUVHU25JWECNEBKIBZFH6N5EUAM42XM3BSEOTMCI3TQDNS5EKLXA_img.jpg)
