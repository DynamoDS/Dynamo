﻿using Dynamo.Models;
using Migrations;

namespace Dynamo.Nodes
{
    class RayBounce : MigrationNode
    {
        [NodeMigration(from: /*NXLT*/"0.6.3.0", to: /*NXLT*/"0.7.0.0")]
        public static NodeMigrationData Migrate_0630_to_0700(NodeMigrationData data)
        {
            return MigrateToDsFunction(data,/*NXLT*/"RevitNodes.dll",
                /*NXLT*/"RayBounce.ByOriginDirection",
                /*NXLT*/"Revit.References.RayBounce.ByOriginDirection@Autodesk.DesignScript.Geometry.Point,Autodesk.DesignScript.Geometry.Vector,int,Revit.Elements.Views.View3D");
        }
    }
}
