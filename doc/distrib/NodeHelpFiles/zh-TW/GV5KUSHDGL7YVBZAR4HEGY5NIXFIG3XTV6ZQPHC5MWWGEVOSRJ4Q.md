## 深入資訊
The ‘Curve Mapper’ node leverages mathematical curves to redistribute points within a defined range. Redistribution in this context means reassigning x-coordinates to new positions along a specified curve based on their y-coordinates. This technique is particularly valuable for applications such as façade design, parametric roof structures, and other design calculations where specific patterns or distributions are required.

透過設定最小值和最大值，定義 X 與 Y 座標的限制。這些限制設定這些點將在當中重新分佈的邊界。接下來，從提供的選項中選取一條數學曲線，其中包括線性曲線、正弦曲線、餘弦曲線、柏林噪波曲線、Bezier 曲線、高斯曲線、拋物線曲線、平方根曲線和冪曲線。使用互動式控制點，根據您的特定需求調整所選曲線的形狀。

您可以使用鎖頭按鈕鎖住曲線形狀，這樣可避免進一步修改曲線。此外，您還可以使用節點內的重設按鈕，將形狀重設為預設狀態。

透過設定 Count 輸入指定要重新分佈的點數。節點會根據選取的曲線和定義的限制，針對指定的點數計算新的 x 座標。點重新分佈的方式為，沿著 y 軸跟著曲線形狀移動而得到 x 座標。

For example, to redistribute 80 points along a sine curve, set Min X to 0, Max X to 20, Min Y to 0, and Max Y to 10. After selecting the sine curve and adjusting its shape as needed, the ‘Curve Mapper’ node outputs 80 points with x-coordinates that follow the sine curve pattern along the y-axis from 0 to 10.


___
## 範例檔案


