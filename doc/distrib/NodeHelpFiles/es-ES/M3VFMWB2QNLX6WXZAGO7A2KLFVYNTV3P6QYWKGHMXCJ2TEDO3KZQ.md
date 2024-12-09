## In-Depth
`TSplineSurface.BuildPipes` genera una superficie con tuberías de T-Spline mediante una red de curvas. Las tuberías individuales se consideran unidas si sus puntos finales se encuentran dentro de la tolerancia máxima establecida por la entrada `snappingTolerance`. El resultado de este nodo puede ajustarse con un conjunto de entradas que permiten establecer valores para todas las tuberías o individualmente si la entrada es una lista de longitud igual al número de tuberías. Las siguientes entradas pueden utilizarse de este modo: `segmentsCount`, `startRotations`, `endRotations`, `startRadii`, `endRadii`, `startPositions` y `endPositions`.

En el ejemplo siguiente, se proporcionan tres curvas unidas por los extremos como entrada para el nodo `TSplineSurface.BuildPipes`. En este caso, `defaultRadius` es un único valor para las tres tuberías, que define el radio de las tuberías por defecto a menos que se proporcionen los radios inicial y final.
A continuación, `segmentsCount` establece tres valores diferentes para cada tubería individual. La entrada es una lista de tres valores, cada uno correspondiente a una tubería.

Hay más ajustes disponibles si `autoHandleStart` y `autoHandleEnd` se establecen en "False" (falso). Esto permite controlar las rotaciones inicial y final de cada tubería (entradas `startRotations` y `endRotations`), así como los radios al final y al principio de cada tubería, especificando los valores para `startRadii` y `endRadii`. Por último, las entradas `startPositions` y `endPositions` permiten desplazar los segmentos al inicio o al final de cada curva, respectivamente. Esta entrada espera un valor correspondiente al parámetro de la curva donde comienzan o terminan los segmentos (valores entre 0 y 1).

## Archivo de ejemplo
![Example](./M3VFMWB2QNLX6WXZAGO7A2KLFVYNTV3P6QYWKGHMXCJ2TEDO3KZQ_img.gif)
