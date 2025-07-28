## 상세
Replace By Condition은 지정된 리스트를 가져와 각 항목을 지정된 조건으로 평가합니다. 조건이 'true'로 평가되면 해당 항목은 출력 리스트에서 replaceWith 입력에 지정된 항목으로 치환됩니다. 아래 예에서는 Formula 노드를 사용하고 공식 x%2==0을 입력합니다. 이 공식은 2로 나눈 후 지정된 항목의 나머지를 구한 다음, 나머지가 0과 같은지 확인합니다. 이 공식은 짝수 정수에 대해 'true'를 반환합니다. 입력 x는 공백으로 남아 있습니다. 이 공식을 ReplaceByCondition 노드의 조건으로 사용하면 각 짝수 수가 지정된 항목(이 경우에는 정수 10)으로 치환되는 출력 리스트가 생성됩니다.
___
## 예제 파일

![ReplaceByCondition](./CoreNodeModels.HigherOrder.Replace_img.jpg)

