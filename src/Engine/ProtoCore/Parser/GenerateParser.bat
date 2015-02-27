copy/b atg\Start.atg+atg\Associative.atg+atg\Imperative.atg+atg\End.atg DesignScript.atg 
Coco.exe -namespace ProtoCore.DesignScriptParser DesignScript.atg
copy DesignScript.atg DS.DesignScript.atg.temp
del DesignScript.atg
PAUSE