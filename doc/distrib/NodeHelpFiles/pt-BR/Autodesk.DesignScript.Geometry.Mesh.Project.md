## Em profundidade
'Mesh.Project' retorna um ponto na malha de entrada que é uma projeção do ponto de entrada na malha na direção do vetor fornecido. Para que o nó funcione corretamente, uma linha desenhada do ponto de entrada na direção do vetor de entrada deve fazer interseção com a malha fornecida.

O gráfico de exemplo mostra um caso de uso simples de como o nó funciona. O ponto de entrada está acima de uma malha esférica, mas não diretamente na parte superior. O ponto é projetado na direção do vetor negativo do eixo “Z”. O ponto resultante é projetado na esfera e aparece logo abaixo do ponto de entrada. Isso é contrastado com a saída do nó 'Mesh.Nearest' (usando o mesmo ponto e malha como entradas), em que o ponto resultante está na malha ao longo do “vetor normal” que passa pelo ponto de entrada (o ponto mais próximo). 'Line.ByStartAndEndPoint' é usado para mostrar a 'trajetória' do ponto projetado na malha.

## Arquivo de exemplo

![Example](./Autodesk.DesignScript.Geometry.Mesh.Project_img.jpg)
