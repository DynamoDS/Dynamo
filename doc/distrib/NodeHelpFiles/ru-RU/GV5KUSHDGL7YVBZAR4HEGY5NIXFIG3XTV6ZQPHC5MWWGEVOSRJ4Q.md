## Подробности
The ‘Curve Mapper’ node leverages mathematical curves to redistribute points within a defined range. Redistribution in this context means reassigning x-coordinates to new positions along a specified curve based on their y-coordinates. This technique is particularly valuable for applications such as façade design, parametric roof structures, and other design calculations where specific patterns or distributions are required.

Определите лимиты для координат X и Y, установив минимальное и максимальное значения. Эти лимиты задают границы, в пределах которых будет происходить перераспределение точек. Затем выберите из предложенных вариантов математическую кривую: линейную кривую, синусоиду, косинусоиду, кривую шума Перлина, кривую Безье, кривую Гаусса, параболу, кривую квадратного корня или кривую степени. Используйте интерактивные управляющие точки, чтобы настроить форму выбранной кривой в соответствии с вашими потребностями.

С помощью кнопки блокировки можно зафиксировать форму кривой, чтобы предотвратить ее дальнейшее изменение. Кроме того, можно восстановить состояние формы по умолчанию с помощью кнопки сброса внутри узла.

Укажите количество точек для перераспределения, задав входной параметр Count. Узел вычисляет новые координаты X для заданного количества точек на основе выбранной кривой и заданных лимитов. Точки перераспределяются таким образом, чтобы их координаты X повторяли форму кривой по оси Y.

For example, to redistribute 80 points along a sine curve, set Min X to 0, Max X to 20, Min Y to 0, and Max Y to 10. After selecting the sine curve and adjusting its shape as needed, the ‘Curve Mapper’ node outputs 80 points with x-coordinates that follow the sine curve pattern along the y-axis from 0 to 10.


___
## Файл примера


