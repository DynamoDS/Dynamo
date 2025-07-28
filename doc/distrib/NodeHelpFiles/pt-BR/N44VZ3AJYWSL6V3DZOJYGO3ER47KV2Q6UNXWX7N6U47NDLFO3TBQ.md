<!--- Autodesk.DesignScript.Geometry.TSpline.TSplineVertex.FunctionalValence --->
<!--- N44VZ3AJYWSL6V3DZOJYGO3ER47KV2Q6UNXWX7N6U47NDLFO3TBQ --->
## In-Depth
A valência funcional de um vértice vai além de uma simples contagem de arestas adjacentes e leva em conta as linhas de grade virtuais que impactam a mesclagem do vértice na área ao redor dele. Ela fornece uma compreensão mais sutil de como os vértices e suas arestas influenciam a superfície durante as operações de deformação e refinamento.
Quando usado em vértices regulares e pontos T, o nó `TSplineVertex.FunctionalValence` retorna o valor de “4”, o que significa que a superfície é guiada por splines em uma forma de grade. Uma valência funcional de qualquer valor diferente de “4” significa que o vértice é um ponto de estrela e a mesclagem em torno do vértice será menos suave.

No exemplo abaixo, o nó `TSplineVertex.FunctionalValence` é usado em dois vértices do ponto T de uma superfície de plano da T-Spline. O nó `TSplineVertex.Valence` retorna o valor de 3, enquanto a valência funcional dos vértices selecionados é 4, que é específica para pontos T. Os nós `TSplineVertex.UVNFrame` e `TSplineUVNFrame.Position` são usados para visualizar a posição dos vértices que estão sendo analisados.

## Arquivo de exemplo

![Example](./N44VZ3AJYWSL6V3DZOJYGO3ER47KV2Q6UNXWX7N6U47NDLFO3TBQ_img.jpg)
