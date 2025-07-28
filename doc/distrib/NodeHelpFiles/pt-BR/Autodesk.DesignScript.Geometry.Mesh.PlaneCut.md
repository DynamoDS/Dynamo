## Em profundidade
'Mesh.PlaneCut' retorna uma malha que foi cortada por um plano especificado. O resultado do corte é a parte da malha situada no lado do plano na direção da normal da entrada 'plane'. O parâmetro 'makeSolid' controla se a malha é tratada como um “Sólido”, caso em que o corte é preenchido com o menor número possível de triângulos para cobrir cada furo.

No exemplo abaixo, uma malha oca obtida de uma operação 'Mesh.BooleanDifference' é cortada por um plano em um ângulo.

## Arquivo de exemplo

![Example](./Autodesk.DesignScript.Geometry.Mesh.PlaneCut_img.jpg)
