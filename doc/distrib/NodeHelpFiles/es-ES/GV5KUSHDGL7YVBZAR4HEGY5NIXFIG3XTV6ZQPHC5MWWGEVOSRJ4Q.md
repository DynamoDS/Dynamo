## En detalle
El nodo `Curve Mapper` aprovecha las curvas matemáticas para redistribuir puntos dentro de un rango definido. En este contexto, la redistribución consiste en reasignar las coordenadas X a nuevas posiciones a lo largo de una curva especificada en función de las coordenadas Y. Esta técnica es especialmente valiosa para aplicaciones como el diseño de fachadas, estructuras paramétricas de cubiertas y otros cálculos de diseño en los que se requieren patrones o distribuciones específicos.

Defina los límites de las coordenadas X e Y mediante la configuración de los valores mínimo y máximo. Estos límites establecen los contornos dentro de los cuales se redistribuirán los puntos. A continuación, seleccione una curva matemática de entre las opciones proporcionadas, que incluyen las curvas lineal, sinusoidal, de coseno, de ruido de Perlin, de Bezier, gaussiana, parabólica, de raíz cuadrada y de potencia. Utilice los puntos de control interactivos para ajustar la forma de la curva seleccionada, adaptándola a sus necesidades específicas.

Puede bloquear la forma de la curva mediante el botón de bloqueo, que impide realizar más modificaciones en la curva. Además, puede restablecer la forma a su estado por defecto mediante el botón de restablecimiento dentro del nodo.

Especifique el número de puntos que se van a redistribuir mediante la entrada `Count`. El nodo calcula nuevas coordenadas X para el número especificado de puntos en función de la curva seleccionada y los límites definidos. Los puntos se redistribuyen de forma que sus coordenadas X sigan la forma de la curva a lo largo del eje Y.

Por ejemplo, para redistribuir 80 puntos a lo largo de una curva sinusoidal, establezca el valor mínimo de X en 0, el valor máximo de X en 20, el valor mínimo de Y en 0 y el valor máximo de Y en 10. Tras seleccionar la curva sinusoidal y ajustar su forma según sea necesario, el nodo `Curve Mapper` emite 80 puntos con coordenadas X que siguen el patrón de la curva sinusoidal a lo largo del eje Y de 0 a 10.




___
## Archivo de ejemplo

![Example](./GV5KUSHDGL7YVBZAR4HEGY5NIXFIG3XTV6ZQPHC5MWWGEVOSRJ4Q_img.jpg)
