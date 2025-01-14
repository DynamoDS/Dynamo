<!--- Autodesk.DesignScript.Geometry.Surface.Thicken(surface, thickness, both_sides) --->
<!--- 5HLUQKT3UZOAWPJMHUXPRYXIG5HOMTLY5RMTZVDGAABIO5MZ3OVQ --->
## Em profundidade
`Surface.Thicken (surface, thickness, both_sides)` cria um sólido deslocando uma superfície de acordo com a entrada `thickness` e tampando as extremidades para fechar o sólido. Esse nó possui uma entrada extra para especificar se deve ou não engrossar em ambos os lados. A entrada `both_sides` assume um valor booleano: True para engrossar em ambos os lados e False para engrossar em um lado. Observe que o parâmetro `thickness` determina a espessura total do sólido final, portanto, se `both_sides` estiver definido como True, o resultado será deslocado da superfície original pela metade da espessura de entrada em ambos os lados.

No exemplo abaixo, primeiro criamos uma superfície usando `Surface.BySweep2Rails`. Em seguida, criamos um sólido usando um controle deslizante numérico para determinar a entrada `thickness` de um nó `Surface.Thicken`. Uma alternância booleana controla se deve engrossar em ambos os lados ou apenas em um.

___
## Arquivo de exemplo

![Surface.Thicken](./5HLUQKT3UZOAWPJMHUXPRYXIG5HOMTLY5RMTZVDGAABIO5MZ3OVQ_img.jpg)
