## Подробности
`Geometry.DeserializeFromSABWithUnits` импортирует геометрию в Dynamo из массива байтов SAB (Standard ACIS Binary) и `DynamoUnit.Unit`, который конвертируется из миллиметров. Этот узел принимает byte[] в качестве первого входного параметра и `dynamoUnit` — в качестве второго. Если входной параметр `dynamoUnit` равен NULL, геометрия SAB импортируется без единиц измерения, а геометрические данные импортируются в массив без преобразования единиц измерения. Если единица измерения указана, внутренние единицы измерения массива SAB преобразуются в указанные единицы.

В Dynamo не используются единицы измерения, однако числовые значения на графике Dynamo, скорее всего, по-прежнему имеют неявные единицы измерения. Входное значение `dynamoUnit` можно использовать для масштабирования внутренней геометрии SAB до этой системы единиц измерения.

В приведенном ниже примере из SAB генерируется кубоид с двумя единицами измерения (без единиц измерения). Входной параметр `dynamoUnit` используется для масштабирования выбранной единицы измерения в других программах.

___
## Файл примера

![Geometry.DeserializeFromSABWithUnits](./GeometryUI.DeserializeFromSABWithUnits_img.jpg)
