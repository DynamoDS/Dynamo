## 상세
The ‘Curve Mapper’ node leverages mathematical curves to redistribute points within a defined range. Redistribution in this context means reassigning x-coordinates to new positions along a specified curve based on their y-coordinates. This technique is particularly valuable for applications such as façade design, parametric roof structures, and other design calculations where specific patterns or distributions are required.

x와 y 좌표에 대한 최소값과 최대값을 설정하여 제한을 정의합니다. 이러한 제한은 점이 재분배될 경계 범위를 설정합니다. 다음으로, 제공된 옵션에서 수학적 곡선(선형, 사인, 코사인, 펄린 노이즈, 베지에, 가우스, 포물선, 제곱근, 거듭제곱 곡선)을 선택합니다. 선택한 곡선의 모양을 조정하기 위해 인터랙티브 제어점을 사용하여 곡선의 모양을 필요에 맞게 맞춥니다.

잠금 버튼으로 곡선 모양을 잠가 곡선이 더 이상 수정되지 않도록 할 수 있습니다. 또한 노드 내의 재설정 버튼을 사용하여 모양을 기본 상태로 재설정할 수도 있습니다.

재분배할 점의 수를 설정하여 Count 입력을 지정합니다. 이 노드는 선택한 곡선과 정의된 제한을 기반으로 지정된 점의 수에 대한 새로운 x 좌표를 계산합니다. 점은 x 좌표가 y 축을 따라 곡선 모양을 따르는 방식으로 재분배됩니다.

For example, to redistribute 80 points along a sine curve, set Min X to 0, Max X to 20, Min Y to 0, and Max Y to 10. After selecting the sine curve and adjusting its shape as needed, the ‘Curve Mapper’ node outputs 80 points with x-coordinates that follow the sine curve pattern along the y-axis from 0 to 10.


___
## 예제 파일


