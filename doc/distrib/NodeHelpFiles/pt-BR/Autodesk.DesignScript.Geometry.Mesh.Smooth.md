## Em profundidade
Esse nó retorna uma nova malha suave usando um algoritmo de suavização cotangente que não espalha os vértices de sua posição original e é melhor para preservar recursos e arestas. Um valor de escala precisa ser inserido no nó para definir a escala espacial da suavização. Os valores de escala podem variar entre 0,1 e 64,0. Valores mais altos resultam em um efeito de suavização mais perceptível, resultando no que parece ser uma malha mais simples. Apesar de parecer mais suave e simples, a nova malha tem a mesma contagem de triângulos, arestas e vértices da inicial.

No exemplo abaixo, 'Mesh.ImportFile' é usado para importar um objeto. 'Mesh.Smooth' é usado para suavizar o objeto, com uma escala de suavização de 5. O objeto é então convertido em outra posição com 'Mesh.Translate' para uma melhor visualização, e 'Mesh.TriangleCount' é usado para rastrear o número de triângulos na malha antiga e na malha nova.

## Arquivo de exemplo

![Example](./Autodesk.DesignScript.Geometry.Mesh.Smooth_img.jpg)
