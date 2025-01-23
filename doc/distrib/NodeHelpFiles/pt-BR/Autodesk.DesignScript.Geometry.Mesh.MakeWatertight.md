## Em profundidade
'Mesh.MakeWatertight' gera uma malha sólida, hermética e que pode ser impressa em 3D por amostragem da malha original. Ele fornece uma maneira rápida de resolver uma malha com vários problemas, como autointerseções, sobreposições e geometria não múltipla. O método calcula um campo de distância de banda fina e gera uma nova malha usando o algoritmo de cubos de marcha, mas não é projetado de volta na malha original. Isso é mais adequado para objetos de malha que têm vários defeitos ou problemas difíceis, como autointerseções.
O exemplo abaixo mostra um vaso não hermético e seu equivalente hermético.

## Arquivo de exemplo

![Example](./Autodesk.DesignScript.Geometry.Mesh.MakeWatertight_img.jpg)
