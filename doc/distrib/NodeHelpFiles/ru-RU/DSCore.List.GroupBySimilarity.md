## Подробности
Узел `List.GroupBySimilarity` группирует элементы списка на основе смежности их индексов и сходства их значений. Список объединяемых в кластер элементов может содержать либо числа (целые и с плавающей точкой), либо строки, но не значения обоих типов.

Используйте входное значение допуска `tolerance` для определения сходства элементов. Для списков чисел значение 'tolerance' представляет собой максимально допустимую разницу между двумя числами, чтобы они считались сходными.

For lists of strings, this value (between 0 and 10) represents the minimum similarity ratio (computed using fuzzy logic) for adjacent elements to be considered similar. For string lists, 'tolerance' represents the maximum number of characters that can differ between two strings, using Levenshtein distance for comparison. Maximum tolerance for strings is set to 10.

Логический входной параметр `considerAdjacency` указывает, следует ли учитывать смежность при группировании элементов. Если задано значение True, в кластер будут включены только схожие смежные элементы. Если задано значение False, для формирования кластеров будет использоваться только сходство, независимо от смежности.

Этот узел выводит список списков группированных значений на основе смежности и сходства, а также список списков индексов группированных элементов в исходном списке.

В приведенном ниже примере узел `List.GroupBySimilarity` используется двумя способами: для кластеризации списка строк только по сходству и для кластеризации списка чисел по смежности и схожести.
___
## Файл примера

![List.GroupBySimilarity](./DSCore.List.GroupBySimilarity_img.jpg)
