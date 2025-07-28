## En detalle
El nodo `Curve Mapper` redistribuye una serie de valores de entrada dentro de un rango definido y aprovecha las curvas matemáticas para asignarlos a lo largo de una curva especificada. En este contexto, la asignación hace referencia a que los valores se redistribuyen de forma que sus coordenadas X sigan la forma de la curva a lo largo del eje Y.  Esta técnica es especialmente valiosa para aplicaciones como el diseño de fachadas, estructuras paramétricas de cubiertas y otros cálculos de diseño en los que se requieren patrones o distribuciones específicos.

Defina los límites para las coordenadas X mediante el establecimiento de los valores mínimo y máximo. Estos límites determinan los contornos dentro de los cuales se redistribuirán los puntos. Puede proporcionar un recuento único para generar una serie de valores distribuidos uniformemente o una serie de valores existente, que se distribuirá a lo largo de la dirección X dentro del intervalo especificado y, a continuación, se asignará a la curva.

Seleccione una curva matemática de entre las opciones proporcionadas, que incluyen curvas lineales, seno, coseno, de ruido de Perlin, de Bezier, gaussianas, parabólicas, de raíz cuadrada y de potencia. Utilice los puntos de control interactivos para ajustar la forma de la curva seleccionada, adaptándola a sus necesidades específicas.

Puede bloquear la forma de la curva mediante el botón de bloqueo para impedir modificaciones posteriores en ella. Además, puede restablecer la forma a su estado por defecto mediante el botón de restablecimiento dentro del nodo. Si obtiene "NaN" o "Null" como salidas, puede encontrar más información [aquí](https://dynamobim.org/introducing-the-curve-mapper-node-in-dynamo/#CurveMapper_Known_Issues) sobre el motivo por el que puede aparecer esto.

Por ejemplo, para redistribuir 80 puntos a lo largo de una curva seno dentro del intervalo de 0 a 20, ajuste los valores "Min" a 0, "Máx" a 20 y "Values" a 80. Tras seleccionar la curva seno y ajustar su forma según sea necesario, el nodo `Curve Mapper` emite 80 puntos con coordenadas X que siguen el patrón de la curva seno a lo largo del eje Y.

Para asignar valores distribuidos de forma desigual a lo largo de una curva gaussiana, establezca el rango mínimo y máximo y proporcione la serie de valores. Tras seleccionar la curva de Gauss y ajustar su forma según sea necesario, el nodo `Curve Mapper` redistribuye la serie de valores a lo largo de las coordenadas X mediante el rango especificado y asigna los valores a lo largo del patrón de la curva. Para obtener documentación detallada sobre cómo funciona el nodo y cómo configurar las entradas, consulte [esta entrada del blog](https://dynamobim.org/introducing-the-curve-mapper-node-in-dynamo) centrada en Curve Mapper.




___
## Archivo de ejemplo

![Example](./GV5KUSHDGL7YVBZAR4HEGY5NIXFIG3XTV6ZQPHC5MWWGEVOSRJ4Q_img.png)
