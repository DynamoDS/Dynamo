using System;

namespace DSRevitNodes
{
    class DSLevel
    {
        static Autodesk.Revit.DB.Level ByElevationAndName(double elevation, string name)
        {
            throw new NotImplementedException();
        }

        static Autodesk.Revit.DB.Level ByLevelAndOffset(DSLevel l, double offset)
        {
            throw new NotImplementedException();
        }
    }
}

/*
[NodeName("Level")]
[NodeCategory(BuiltinNodeCategories.REVIT_DATUMS)]
[NodeDescription("Creates a level datum")]
public class Level : RevitTransactionNodeWithOneOutput
{
    public Level()
    {
        InPortData.Add(new PortData("elevation", "The elevation of the level.", typeof(FScheme.Value.Number)));
        InPortData.Add(new PortData("name", "The name of the level.", typeof(FScheme.Value.String)));

        OutPortData.Add(new PortData("level", "The level.", typeof(FScheme.Value.Container)));

        RegisterAllPorts();
    }

    public override FScheme.Value Evaluate(FSharpList<FScheme.Value> args)
    {
        //Level elements take in one double for the z elevation (height)f
        var h = (double)((FScheme.Value.Number)args[0]).Item;
        var name = ((FScheme.Value.String)args[1]).Item;

        Autodesk.Revit.DB.Level lev;

        if (this.Elements.Any())
        {
            if (dynUtils.TryGetElement(this.Elements[0], out lev))
            {
                lev.Elevation = h;
                lev.Name = name;
            }
            else
            {
                lev = this.UIDocument.Document.IsFamilyDocument
                    ? this.UIDocument.Document.FamilyCreate.NewLevel(h)
                    : this.UIDocument.Document.Create.NewLevel(h);
                lev.Name = name;
                this.Elements[0] = lev.Id;
            }
        }
        else
        {
            lev = this.UIDocument.Document.IsFamilyDocument
                ? this.UIDocument.Document.FamilyCreate.NewLevel(h)
                : this.UIDocument.Document.Create.NewLevel(h);
            lev.Name = name;
            this.Elements.Add(lev.Id);
        }

        return FScheme.Value.NewContainer(lev);
    }
}
*/