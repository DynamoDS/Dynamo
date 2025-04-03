## En detalle
The ‘Curve Mapper’ node leverages mathematical curves to redistribute points within a defined range. Redistribution in this context means reassigning x-coordinates to new positions along a specified curve based on their y-coordinates. This technique is particularly valuable for applications such as façade design, parametric roof structures, and other design calculations where specific patterns or distributions are required.

Defina los límites de las coordenadas X e Y mediante la configuración de los valores mínimo y máximo. Estos límites establecen los contornos dentro de los cuales se redistribuirán los puntos. A continuación, seleccione una curva matemática de entre las opciones proporcionadas, que incluyen las curvas lineal, sinusoidal, de coseno, de ruido de Perlin, de Bezier, gaussiana, parabólica, de raíz cuadrada y de potencia. Utilice los puntos de control interactivos para ajustar la forma de la curva seleccionada, adaptándola a sus necesidades específicas.

Puede bloquear la forma de la curva mediante el botón de bloqueo, que impide realizar más modificaciones en la curva. Además, puede restablecer la forma a su estado por defecto mediante el botón de restablecimiento dentro del nodo.

Especifique el número de puntos que se van a redistribuir mediante la entrada `Count`. El nodo calcula nuevas coordenadas X para el número especificado de puntos en función de la curva seleccionada y los límites definidos. Los puntos se redistribuyen de forma que sus coordenadas X sigan la forma de la curva a lo largo del eje Y.

For example, to redistribute 80 points along a sine curve, set Min X to 0, Max X to 20, Min Y to 0, and Max Y to 10. After selecting the sine curve and adjusting its shape as needed, the ‘Curve Mapper’ node outputs 80 points with x-coordinates that follow the sine curve pattern along the y-axis from 0 to 10.


___
## Archivo de ejemplo


