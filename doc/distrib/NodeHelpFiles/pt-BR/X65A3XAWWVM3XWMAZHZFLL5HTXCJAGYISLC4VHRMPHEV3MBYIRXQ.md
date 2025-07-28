<!--- Autodesk.DesignScript.Geometry.Solid.BySweep(profile, path, cutEndOff) --->
<!--- X65A3XAWWVM3XWMAZHZFLL5HTXCJAGYISLC4VHRMPHEV3MBYIRXQ --->
## Em profundidade
`Solid.BySweep` cria um sólido varrendo uma curva de perfil fechada de entrada ao longo de um caminho especificado.

No exemplo abaixo, usamos um retângulo como curva do perfil base. O caminho é criado usando uma função de cosseno com uma sequência de ângulos para variar as coordenadas x de um conjunto de pontos. Os pontos são usados como entrada para um nó `NurbsCurve.ByPoints`. Em seguida, criamos um sólido varrendo o retângulo ao longo da curva de cosseno criada.
___
## Arquivo de exemplo

![Solid.BySweep](./X65A3XAWWVM3XWMAZHZFLL5HTXCJAGYISLC4VHRMPHEV3MBYIRXQ_img.jpg)
