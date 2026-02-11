## 상세
'List.AllFalse'는 지정된 리스트의 항목이 True이거나 부울이 아닌 경우 False를 반환합니다. 'List.AllFalse'는 지정된 리스트의 모든 항목이 부울이고 False인 경우에만 True를 반환합니다.

아래 예에서는 'List.AllFalse'를 사용하여 부울 값 리스트를 평가합니다. 첫 번째 리스트에는 True 값이 있으므로 False가 반환됩니다. 두 번째 리스트에는 False 값만 있으므로 True가 반환됩니다. 세 번째 리스트에는 True 값이 포함된 하위 리스트가 있으므로 False가 반환됩니다. 마지막 노드는 두 하위 리스트를 평가하고, 첫 번째 하위 리스트의 경우 True 값이 있으므로 False를 반환하고 두 번째 하위 리스트의 경우 False 값만 있으므로 True를 반환합니다.
___
## 예제 파일

![List.AllFalse](./DSCore.List.AllFalse_img.jpg)
