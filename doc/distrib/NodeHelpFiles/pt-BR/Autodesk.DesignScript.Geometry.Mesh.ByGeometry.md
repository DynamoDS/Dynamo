## Em profundidade
'Mesh.ByGeometry' usa objetos de geometria do Dynamo (superfícies ou sólidos) como entrada e os converte em uma malha. Pontos e curvas não têm representações de malha, portanto, não são entradas válidas. A resolução da malha produzida na conversão é controlada pelas duas entradas: 'tolerance' e 'maxGridLines'. A entrada 'tolerance' define o desvio aceitável da malha em relação à geometria original e está sujeita ao tamanho da malha. Se o valor de 'tolerance' estiver definido como -1, o Dynamo escolherá uma tolerância sensível. A entrada 'maxGridLines' define o número máximo de linhas de grade na direção U ou V. Um número maior de linhas de grade ajuda a aumentar a suavidade do serrilhado.

## Arquivo de exemplo

![Example](./Autodesk.DesignScript.Geometry.Mesh.ByGeometry_img.jpg)
