#region Copyright
//      .NET Sample
//
//      Copyright (c) 2007 by Autodesk, Inc.
//
//      Permission to use, copy, modify, and distribute this software
//      for any purpose and without fee is hereby granted, provided
//      that the above copyright notice appears in all copies and
//      that both that copyright notice and the limited warranty and
//      restricted rights notice below appear in all supporting
//      documentation.
//
//      AUTODESK PROVIDES THIS PROGRAM "AS IS" AND WITH ALL FAULTS.
//      AUTODESK SPECIFICALLY DISCLAIMS ANY IMPLIED WARRANTY OF
//      MERCHANTABILITY OR FITNESS FOR A PARTICULAR USE.  AUTODESK, INC.
//      DOES NOT WARRANT THAT THE OPERATION OF THE PROGRAM WILL BE
//      UNINTERRUPTED OR ERROR FREE.
//
//      Use, duplication, or disclosure by the U.S. Government is subject to
//      restrictions set forth in FAR 52.227-19 (Commercial Computer
//      Software - Restricted Rights) and DFAR 252.227-7013(c)(1)(ii)
//      (Rights in Technical Data and Computer Software), as applicable.
#endregion

#region Namespaces
using System;
using System.Resources;
using System.Reflection;
using System.Globalization;
using Autodesk.Aec.SpaceOffsetRulesManager;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Runtime;
using Autodesk.Aec.Structural.DatabaseServices;
using Autodesk.Aec.Arch.DatabaseServices;
#endregion

[assembly: ExtensionApplication(null)]

namespace AecSpaceOffsetStandardSample
{
#region AecSpaceOffsetStandardSample
    ///////////////////////////////////////////////////////////////////////////////
    //
    //  Sample Standard
    //  ==============
    //
    //  Description:
    //  ------------
    //
    //  The sample standard provides general rules to offset gross and boundaries
    //  from base boundary which are described below.
    //
    //  Calculation Rules
    //  -----------------
    //
    //  =========================================================================
    //                  | Space          | Bounding | Adjacent | Boundary
    //                  | Classification | Objects  | Space    |
    //  =========================================================================
    //   Net Boundary   | --             | Door     | --       | Panel face of
    //                  |                |          |          | door
    //  -------------------------------------------------------------------------
    //   Net Boundary   | --             | --       | --       | Adjacent face
    //  -------------------------------------------------------------------------
    //   Gross Boundary | --             | --       | None     | Opposite face
    //  -------------------------------------------------------------------------
    //   Gross Boundary | --             | --       | Any      | Centerline
    //  =========================================================================
    //
    ///////////////////////////////////////////////////////////////////////////////

    public class AecSpaceOffsetStandardSample : AecSpaceOffsetStandard
    {

        //  The constructor sets the properties for this standard
        //
        public AecSpaceOffsetStandardSample()
        {

            //  set the properties
            //
            SetName("Sample Standard");
        }

        //  Registers the rules with this standard, so the offset
        //  calculation algorithm can call them for the according boundary type
        //
        protected override void InitRules()
        {
            //  Register the rules with this standard, so the
            //  offset calculation algorithm can call them
            //  for the according boundary type
            NetRules().AppendBoundingOpeningRule(new BoundingOpeningRuleNet());

            //  no usable boundary
            UsableRules().SetSpaceRule(new NullSpace());

            GrossRules().AppendBoundingObjectRule(new BoundingObjectRuleGross());
            GrossRules().AppendBoundingAdjacencyRule(new BoundingAdjacencyRuleGross());
        }
    }
#endregion

#region BoundingObjectRuleGross
    public class BoundingObjectRuleGross : AecBoundingObjectRule
    {
        public BoundingObjectRuleGross()
        {
            RegisterType(typeof(kAllTypes));         
        }

        public override bool Apply(ObjectId idSpace, ObjectId idObject)
        {
            bool result = true;

            Autodesk.AutoCAD.DatabaseServices.Database db = idObject.Database;
            using (Autodesk.AutoCAD.DatabaseServices.Transaction transaction = db.TransactionManager.StartTransaction())
            {
                Member member = transaction.GetObject(idObject, OpenMode.ForRead) as Member;

                if (member != null)
                {
                    MemberType memType = member.MemberType;

                    if (memType == MemberType.Column)
                    {
                        result = false;
                    }
                }

                transaction.Commit();
            }

            return result;
        }
    }
#endregion

#region NullSpace
    public class NullSpace : AecSpaceRule
    {
        public NullSpace()
        {
            RegisterType(typeof(Space));
        }

        public override AecSpaceRuleResult Apply(Autodesk.AutoCAD.DatabaseServices.ObjectId idSpace)
        {
            return AecSpaceRuleResult.NoBoundary;
        }
    }
#endregion

#region BoundingOpeningRuleNet
    //  The opening rule for the net offset will
    //  set the boundary to the center of the door panel for all doors
    //
    public class BoundingOpeningRuleNet : AecBoundingOpeningRule
    {
        public BoundingOpeningRuleNet()
        {
            RegisterType(typeof(Door));
        }

        public override AecSpaceOffsetOpeningInfo Apply(ObjectId idSpace, ObjectId idAdjSpace, ObjectId idObject, ObjectId idOpening)
        {
            
            if (!idAdjSpace.IsNull)
            {
                Database db = HostApplicationServices.WorkingDatabase;
                using (Transaction transaction = db.TransactionManager.StartTransaction())
                {
                    Door door = transaction.GetObject(idOpening, OpenMode.ForRead) as Door;
                    if (door != null)
                    {
                        if (door.Width > 30)
                        {
                            transaction.Commit();
                            return new AecSpaceOffsetOpeningInfo(AecSpaceOffsetOpeningType.PanelCenter);
                        }

                    }
                    transaction.Commit();
                }

            }
           
             return new AecSpaceOffsetOpeningInfo(AecSpaceOffsetOpeningType.NotApplicable);
        }
    }
#endregion

#region BoundingAdjacencyRuleGross
    //  The bounding adjacent rule of the gross offset will set all
    //  wall boundaries to the opposite plance if there is no adjacent space.
    //  If there is none, the default setup as set by the bounding plane rule
    //  will be kept
    //
    public class BoundingAdjacencyRuleGross : AecBoundingAdjacencyRule
    {
        public BoundingAdjacencyRuleGross()
        {
            RegisterType(typeof(Space));
        }

        public override AecSpaceOffsetInfo Apply(ObjectId idSpace, ObjectId idObject, ObjectId idAdjSpace)
        {
            if (idAdjSpace.IsNull)
            {
                //  If there is no adjacent space, keep the 'Opposite' offset
                //
                return new AecSpaceOffsetInfo(AecSpaceOffsetType.Opposite);
            }
            else
            {
                //  If there is an adjacent space, its center
                //
                return new AecSpaceOffsetInfo(AecSpaceOffsetType.Center);
            }
        }
    }
#endregion
}
