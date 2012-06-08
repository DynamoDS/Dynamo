using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Autodesk.Revit.DB;
using Dynamo.Utilities;

namespace Dynamo
{
   public delegate void DynElementUpdateDelegate(List<ElementId> updated);

   public enum ChangeTypeEnum
   {
      Delete,
      Modified
   };

   public class DynamoUpdater : IUpdater
   {
      static AddInId m_appId;
      static UpdaterId m_updaterId;

      Dictionary<ChangeTypeEnum, Dictionary<ElementId, DynElementUpdateDelegate>> updateDict
         = new Dictionary<ChangeTypeEnum, Dictionary<ElementId, DynElementUpdateDelegate>>();
      
      //SpatialFieldManager m_sfm = null;
      
      // constructor takes the AddInId for the add-in associated with this updater
      public DynamoUpdater(AddInId id)
      {
         m_appId = id;
         m_updaterId = new UpdaterId(m_appId, new Guid("1F1F44B4-8002-4CC1-8FDB-17ACD24A2ECE")); //[Guid("1F1F44B4-8002-4CC1-8FDB-17ACD24A2ECE")]

         this.updateDict[ChangeTypeEnum.Delete] = new Dictionary<ElementId, DynElementUpdateDelegate>();
         this.updateDict[ChangeTypeEnum.Modified] = new Dictionary<ElementId, DynElementUpdateDelegate>();
      }

      public void RegisterChangeHook(ElementId e, ChangeTypeEnum type, DynElementUpdateDelegate d)
      {
         Dictionary<ElementId, DynElementUpdateDelegate> dict;
         if (!this.updateDict.ContainsKey(type))
         {
            dict = new Dictionary<ElementId, DynElementUpdateDelegate>();
            this.updateDict[type] = dict;
         }
         else
            dict = this.updateDict[type];

         dict[e] = d;
      }

      public void UnRegisterChangeHook(ElementId e, ChangeTypeEnum type)
      {
         this.updateDict[type].Remove(e);
      }

      public void Execute(UpdaterData data)
      {
         //Document doc = data.GetDocument();
         var bench = dynElementSettings.SharedInstance.Bench; // MDJ HOOK


         var modDict = this.updateDict[ChangeTypeEnum.Modified];
         var dict = new Dictionary<DynElementUpdateDelegate, List<ElementId>>();
         foreach (ElementId modifiedElementID in data.GetModifiedElementIds())
         {
            //Element m_modifiedElement = data.GetDocument().get_Element(m_modifiedElementID) as Element;// note the filter should return all ref points, curves and family instances now. 

            try
            {
               if (!modDict.ContainsKey(modifiedElementID))
                  continue;

               var k = modDict[modifiedElementID];
               if (!dict.ContainsKey(k))
                  dict[k] = new List<ElementId>();
               dict[k].Add(modifiedElementID);

               //if (dynElementSettings.SharedInstance.UserSelectedElements.Contains(m_modifiedElement)) // if the element that was updated is contained in the set of elements selected before, force a rebuild of dynamo graph
               //{

               //   if (bench.DynamicRunEnabled && !bench.Running)
               //      bench.RunExpression(false, false); // if it's one we are watching kick off RunExpression (note the pre-queue code from dynWorkspace Modified()

               //   // this is the new new queue-based code as in dynWorkspace Modified() but this causes cyclic behavior from the DMU side
               //   //if (bench.DynamicRunEnabled) 
               //   //{
               //   //    if (!bench.Running)
               //   //        bench.RunExpression(false, false);
               //   //    else
               //   //        bench.QueueRun();
               //   //}
               //}

            }

            catch (Exception e)
            {
               bench.Log(e.ToString());
            }

         }

         foreach (var pair in dict)
         {
            pair.Key(pair.Value);
         }

         modDict = this.updateDict[ChangeTypeEnum.Delete];
         dict.Clear();
         foreach (ElementId deletedElementID in data.GetDeletedElementIds())
         {
            //Element m_deletedElement = data.GetDocument().get_Element(m_deletedElementID) as Element;

            try
            {
               if (!modDict.ContainsKey(deletedElementID))
                  continue;

               var k = modDict[deletedElementID];
               if (!dict.ContainsKey(k))
                  dict[k] = new List<ElementId>();
               dict[k].Add(deletedElementID);

               //if (dynElementSettings.SharedInstance.UserSelectedElements.Contains(m_deletedElement)) // if the element that was updated is contained in the set of elements selected before but was deleted, remove from collection
               //{

               //   dynElementSettings.SharedInstance.UserSelectedElements.Erase(m_deletedElement); // remove deleted element from watch list
               //}

            }

            catch (Exception e)
            {
               bench.Log(e.ToString());
            }

         }

         foreach (var pair in dict)
         {
            pair.Key(pair.Value);
         }
      }

      public string GetAdditionalInformation()
      {
         return "Watch for user-selected elements that have been changed or deleted and use this info to update Dynnamo";
      }
      public ChangePriority GetChangePriority()
      {
         return ChangePriority.FloorsRoofsStructuralWalls;
      }
      public UpdaterId GetUpdaterId()
      {
         return m_updaterId;
      }
      public string GetUpdaterName()
      {
         return "Dyanmo RefPoint Watcher";
      }
   }
}
