## 상세
'List.FilterByBoolMask'는 두 개의 리스트를 입력으로 사용합니다. 첫 번째 리스트는 부울(True 또는 False) 값의 해당 리스트에 따라 두 개의 개별 리스트로 분할됩니다. 'list' 입력에서 'mask' 입력의 True에 해당하는 항목은 'In' 레이블이 지정된 출력으로 전달되고 False 값에 해당하는 항목은 'out' 레이블이 지정된 출력으로 전달됩니다.

아래 예에서 'List.FilterByBoolMask'는 재료 리스트에서 목재 및 라미네이트를 선택하는 데 사용됩니다. 먼저 두 리스트를 비교하여 일치하는 항목을 찾은 다음 'Or' 연산자를 사용하여 True 리스트 항목을 확인합니다. 그러면 리스트 항목은 목재인지 라미네이트인지 또는 다른 재료인지에 따라 필터링됩니다.
___
## 예제 파일

![List.FilterByBoolMask](./DSCore.List.FilterByBoolMask_img.jpg)
