﻿//
// Revit IFC Import library: this library works with Autodesk(R) Revit(R) to import IFC files.
// Copyright (C) 2013  Autodesk, Inc.
// 
// This library is free software; you can redistribute it and/or
// modify it under the terms of the GNU Lesser General Public
// License as published by the Free Software Foundation; either
// version 2.1 of the License, or (at your option) any later version.
//
// This library is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
// Lesser General Public License for more details.
//
// You should have received a copy of the GNU Lesser General Public
// License along with this library; if not, write to the Free Software
// Foundation, Inc., 51 Franklin Street, Fifth Floor, Boston, MA  02110-1301  USA
//

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.IFC;
using Revit.IFC.Common.Utility;
using Revit.IFC.Common.Enums;
using Revit.IFC.Import.Enums;
using Revit.IFC.Import.Geometry;
using Revit.IFC.Import.Utility;

namespace Revit.IFC.Import.Data
{
   /// <summary>
   /// The subtypes represent topological loops.
   /// </summary>
   public abstract class IFCLoop : IFCTopologicalRepresentationItem
   {
      // In the case of a tessellated shape, the IFCLoop is defined by vertices.
      IList<XYZ> m_LoopVertices = null;

      public IList<XYZ> LoopVertices
      {
         get
         {
            if (m_LoopVertices == null)
               m_LoopVertices = GenerateLoopVertices();
            return m_LoopVertices;
         }
      }

      /// <summary>
      /// Checks if the Loop definition represents a non-empty boundary.
      /// </summary>
      /// <returns>True if the FaceBound contains any information.</returns>
      virtual public bool IsEmpty()
      {
         return false;
      }

      protected IFCLoop()
      {
      }

      override protected void Process(IFCAnyHandle ifcLoop)
      {
         base.Process(ifcLoop);
      }

      virtual protected CurveLoop GenerateLoop()
      {
         return null;
      }

      virtual protected IList<XYZ> GenerateLoopVertices()
      {
         return null;
      }

      /// <summary>
      /// Create geometry for a particular representation item.
      /// </summary>
      /// <param name="shapeEditScope">The geometry creation scope.</param>
      /// <param name="lcs">Local coordinate system for the geometry, without scale.</param>
      /// <param name="scaledLcs">Local coordinate system for the geometry, including scale, potentially non-uniform.</param>
      /// <param name="guid">The guid of an element for which represntation is being created.</param>
      protected override void CreateShapeInternal(IFCImportShapeEditScope shapeEditScope, Transform lcs, Transform scaledLcs, string guid)
      {
         base.CreateShapeInternal(shapeEditScope, lcs, scaledLcs, guid);
      }

      protected IFCLoop(IFCAnyHandle ifcLoop)
      {
         Process(ifcLoop);
      }

      /// <summary>
      /// Create an IFCLoop object from a handle of type IfcLoop.
      /// </summary>
      /// <param name="ifcLoop">The IFC handle.</param>
      /// <returns>The IFCLoop object.</returns>
      public static IFCLoop ProcessIFCLoop(IFCAnyHandle ifcLoop)
      {
         if (IFCAnyHandleUtil.IsNullOrHasNoValue(ifcLoop))
         {
            Importer.TheLog.LogNullError(IFCEntityType.IfcLoop);
            return null;
         }

         IFCEntity loop;
         if (IFCImportFile.TheFile.EntityMap.TryGetValue(ifcLoop.StepId, out loop))
            return (loop as IFCLoop);

         if (IFCImportFile.TheFile.SchemaVersionAtLeast(IFCSchemaVersion.IFC2x2) && IFCAnyHandleUtil.IsSubTypeOf(ifcLoop, IFCEntityType.IfcEdgeLoop))
            return IFCEdgeLoop.ProcessIFCEdgeLoop(ifcLoop);

         if (IFCAnyHandleUtil.IsValidSubTypeOf(ifcLoop, IFCEntityType.IfcPolyLoop))
            return IFCPolyLoop.ProcessIFCPolyLoop(ifcLoop);

         Importer.TheLog.LogUnhandledSubTypeError(ifcLoop, IFCEntityType.IfcLoop, false);
         return null;
      }
   }
}