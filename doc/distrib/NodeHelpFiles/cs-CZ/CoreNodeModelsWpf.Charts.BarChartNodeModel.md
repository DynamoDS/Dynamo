## Podrobnosti

Možnost Pruhový graf vytvoří graf s vertikálně orientovanými pruhy. Pruhy je možné uspořádat do více skupin a opatřit popisky s barevným rozlišením. Máte možnost vytvořit jednu skupinu zadáním jedné hodnoty typu double nebo více skupin zadáním více hodnot typu double na každý dílčí seznam u vstupního pole hodnot. Chcete-li definovat kategorie, vložte seznam řetězcových hodnot do vstupního pole popisků. Každá hodnota vytvoří novou barevně rozlišenou kategorii.

Chcete-li přiřadit hodnotu (výšku) ke každému pruhu, zadejte seznam seznamů, které obsahují hodnoty typu double, do vstupního pole hodnot. Každý dílčí seznam určí počet pruhů a kategorii, do které patří, ve stejném pořadí jako ve vstupním poli popisků. Pokud máte jeden seznam hodnot typu double, bude vytvořena pouze jedna kategorie. Počet řetězcových hodnot ve vstupním poli popisků musí odpovídat počtu dílčích seznamů ve vstupním poli hodnot.

Chcete-li každé kategorii přiřadit barvu, vložte seznam barev do vstupního pole barev. Při přiřazování vlastních barev musí počet barev odpovídat počtu řetězcových hodnot ve vstupním poli popisků. Pokud nejsou přiřazeny žádné barvy, použijí se náhodné barvy.

## Příklad: Jedna skupina

Řekněme, že chcete vyjádřit průměrná uživatelská hodnocení za dobu prvních tří měsíců v roce. Chcete-li tuto situaci vizualizovat, potřebujete seznam tří řetězcových hodnot, označených popisky Leden, Únor a Březen.
Do vstupního pole popisků tedy zadáme následující seznam v bloku kódu:

[„Leden“, „Únor“, „Březen“];

Seznam můžete také vytvořit pomocí uzlů String připojených k uzlu List Create.

Dále do vstupního pole hodnot zadáme průměrné uživatelské hodnocení pro každý ze tří měsíců jako seznam seznamů:

[[3.5], [5], [4]];

Všimněte si, že protože máme tři popisky, potřebujeme tři dílčí seznamy.

Nyní, když je graf spuštěn, vytvoří se pruhový graf, přičemž každý barevný pruh bude představovat průměrné zákaznické hodnocení pro daný měsíc. Můžete dále používat výchozí barvy nebo můžete do vstupního pole barev vložit seznam vlastních barev.

## Příklad: Více skupin

Funkci seskupování uzlu pruhového grafu můžete využít zadáním více hodnot do každého dílčího seznamu ve vstupním poli hodnot. V tomto příkladu vytvoříme graf vizualizující počet dveří ve třech variantách tří modelů – Model A, Model B a Model C.

Za tímto účelem nejprve zadáme popisky:

[„Model A“, „Model B“, „Model C“];

Dále zadáme hodnoty a znovu se ujistíme, že počet dílčích seznamů odpovídá počtu popisků:

[[17, 9, 13],[12,11,15],[15,8,17]];

Nyní, když kliknete na tlačítko Spustit, uzel Pruhový graf vytvoří graf se třemi skupinami pruhů označenými indexem 0, 1 a 2. V tomto příkladu považujte každý index (tj. skupinu) za variantu návrhu. Hodnoty v první skupině (index 0) jsou převzaty z první položky v každém seznamu ve vstupním poli hodnot, čili první skupina obsahuje 17 u modelu A, 12 u modelu B a 15 u modelu C. Druhá skupina (index 1) používá druhou hodnotu v každé skupině atd.

___
## Vzorový soubor

![Bar Chart](./CoreNodeModelsWpf.Charts.BarChartNodeModel_img.jpg)

