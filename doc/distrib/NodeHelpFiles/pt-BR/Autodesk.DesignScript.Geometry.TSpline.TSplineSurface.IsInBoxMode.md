## Em profundidade
O modo de caixa e o modo suave são duas formas de visualizar uma superfície da T-Spline. O modo suave é a forma verdadeira de uma superfície da T-Spline e é útil para visualizar a estética e as dimensões do modelo. O modo de caixa, por outro lado, pode fornecer informações sobre a estrutura da superfície e oferecer uma melhor compreensão dela, bem como uma opção mais rápida para visualizar geometria grande ou complexa. Os modos suave e de caixa podem ser controlados no momento da criação da superfície da T-Spline, com nós como `TSplineSurface.EnableSmoothMode`.

Nos casos em que uma T-Spline se torna inválida, sua visualização alterna automaticamente para o modo de caixa. O nó `TSplineSurface.IsInBoxMode` é outra forma de identificar se a superfície se torna inválida.

No exemplo abaixo, é criada uma superfície de plano da T-Spline com a entrada `smoothMode` definida como True. Duas de suas faces são excluídas, tornando a superfície inválida. A visualização da superfície alterna para o modo de caixa, embora seja impossível distinguir somente com base na visualização. O nó `TSplineSurface.IsInBoxMode` é usado para confirmar se a superfície está no modo de caixa.
___
## Arquivo de exemplo

![TSplineSurface.IsInBoxMode](./Autodesk.DesignScript.Geometry.TSpline.TSplineSurface.IsInBoxMode_img.jpg)
