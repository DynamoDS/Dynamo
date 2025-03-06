## Podrobnosti

Uzel Define data ověřuje typ příchozích dat. Lze jej použít k zajištění požadovaného typu místních dat a také jako vstupní nebo výstupní uzel, který určuje typ dat, která graf očekává nebo poskytuje. Uzel podporuje výběr běžně používaných datových typů Dynamo, například „String“ „Point“ nebo „Boolean“. Úplný seznam podporovaných datových typů je k dispozici v rozbalovací nabídce uzlu. Uzel podporuje data ve formě jedné hodnoty nebo plochého seznamu. Vnořené seznamy, slovníky a replikace nejsou podporovány.

### Chování

Uzel ověří data přicházející ze vstupního portu na základě nastavení rozevírací nabídky Type a přepínače List (podrobnosti naleznete níže). Pokud je ověření úspěšné, výstup uzlu je stejný jako vstup. Pokud ověření není úspěšné, uzel přejde do stavu upozornění s výstupem null.
Uzel má jeden vstup:

-   Vstup "**>**" – Připojí se k uzlu proti proudu a ověří jeho datový typ.
    Uzel navíc nabízí tři uživatelské ovládací prvky:
-   Přepínač **Auto-detect type** – Když je zapnutý, uzel analyzuje příchozí data, a pokud jsou data podporovaného typu, uzel nastaví hodnoty ovládacích prvků Type a List na základě typu příchozích dat. Rozbalovací nabídka Type a přepínač List jsou zakázány a automaticky se aktualizují na základě vstupního uzlu.

    Když je automatické rozpoznání typu vypnuto, můžete určit typ dat pomocí nabídky Type a přepínače List. Pokud příchozí data neodpovídají tomu, co jste zadali, uzel přejde do stavu upozornění s výstupem null.
-   Rozbalovací nabídka **Type** – Nastavuje očekávaný datový typ. Když je ovládací prvek povolen (přepínač **Auto-detect type** je vypnutý), nastaví datový typ pro ověření. Když je ovládací prvek zakázán (přepínač **Auto-detect type** je zapnutý), datový typ se nastaví automaticky na základě příchozích dat. Data jsou platná, pokud jejich typ přesně odpovídá zobrazenému typu nebo pokud je jejich typ podřazeným typem zobrazeného typu (například pokud je rozevírací nabídka Type nastavena na „Curve“, objekty typu „Rectangle“, „Line“, atd. jsou platné).
-   Přepínač **List** – Pokud je zapnutý, uzel očekává, že importovaná data budou pouze jedním plochým seznamem obsahujícím položky platného datového typu (viz výše). Pokud je přepínač vypnutý, uzel očekává jednu položku platného datového typu.

### Použít jako vstupní uzel

Když je nastaven jako vstup („Je vstup“ v místní nabídce uzlu), může uzel volitelně použít uzly proti proudu k nastavení výchozí hodnoty pro vstup. Spuštění grafu uloží hodnotu uzlu Define Data do mezipaměti pro použití při externím spuštění grafu, například s uzlem Engine.

## Vzorový soubor

V níže uvedeném příkladu má první skupina uzlů „DefineData“ vypnutý přepínač **Auto-detect type**. Uzel správně ověřuje zadání Number při odmítnutí zadání String. Druhá skupina obsahuje uzel s přepínačem **Auto-detect typ**. Uzel automaticky upraví rozevírací seznam Type a přepínač List tak, aby odpovídaly zadání, v tomto případě seznamu celých čísel.

![Define_Data](./CoreNodeModels.DefineData_img.png)
