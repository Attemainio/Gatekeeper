using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using Grasshopper.GUI;
using Grasshopper.GUI.Canvas;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Data;
using Grasshopper.Kernel.Types;
using Rhino.Commands;

namespace Gatekeeper
{
    public class GH_GatekeeperComponent : GH_Component, IHasDoubleClick, IHasPhase
    {

        /// <summary>
        /// used for the cached data. this is needed if we reconnect the output from this component to new parameters.
        /// </summary>
        private GH_Structure<IGH_Goo> _data = new GH_Structure<IGH_Goo>();
        /// <summary>
        /// true if input parameters are all true.
        /// </summary>
        public bool Compute { get; set; } = false;

        DateTime IHasPhase.LastRun { get => lastRun; set => lastRun = value; }
        DateTime lastRun = DateTime.MinValue;

        private bool wasComputing = true;


        public enum Phases
        {
            Open,
            CloseAndUpdated,
            CloseAndOutdated,
        }

        public Phases Phase { get; set; } = Phases.CloseAndOutdated; 



        private bool SingleRunFromDoubleClick = false;

        /// <summary>
        /// Initializes a new instance of the GH_DataDamComponent class.
        /// </summary>
        public GH_GatekeeperComponent()
          : base("Gatekeeper", "GK",
              "The Gatekeeper component is a powerful tool designed to manage the flow of data within Grasshopper for Rhino. " +
              "It acts as a conditional gate that can prevent data from propagating further in a Grasshopper definition based on a boolean condition, " +
              "without triggering a recomputation of the solution. " +
              "This component is particularly useful when you need to control the data flow based on specific conditions, " +
              "and it ensures a seamless user experience by retaining the previous data state.\n\n" +
                "Double click for a single update",
              "Params", "Util")
        {

        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("Data", "D", "Data", GH_ParamAccess.tree);
            pManager.AddBooleanParameter("Pass", "P", "If this list only contains positive inputs, the gate is open. You can right click and select Revese if you want to invert this (couldnt find how to look for the invert flag...)", GH_ParamAccess.tree, false);
            pManager[0].Optional = true;
            pManager[1].Optional = true;
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Data", "D", "Data", GH_ParamAccess.tree);
            pManager.AddBooleanParameter("Running", "R", "True if it is updated. This is relevant if there's some previews etc later in your canvas that you want to turn on or off regardless of the frozen data", GH_ParamAccess.item);
        }




        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            var time = DateTime.Now;
            Compute = GetFlattenedBooleans(1) || SingleRunFromDoubleClick;

            Message = "";

            if (Params.Output.Count > 1)
                DA.SetData(1, Compute);

            DA.GetDataTree(0, out _data);

            DA.SetDataTree(0, _data);

            
            if (Compute)
            {
                lastRun = time;
                Phase = Phases.Open;
            }
            else
            {
                // Pretty lame way of checking if its outdated. It checks if any of the inputs are changed while its in "false" mode.
                // This can by mistake also be triggered if any of the RUN inputs are changed and not the data itself.
                // Solving this is a hard nut to crack. But works as intended 99% of the cases. And 100% of cases if your trigger is ONE button/toggle.
                if (Phase == Phases.CloseAndUpdated || Phase == Phases.CloseAndOutdated)
                    Phase = Phases.CloseAndOutdated;
                else
                    Phase = Phases.CloseAndUpdated;

            }

        }

        protected override void ExpireDownStreamObjects()
        {
            if (GetFlattenedBooleans(1) || SingleRunFromDoubleClick) // tried looking at compute field, but somehow this needs to rerun, thus calling GetFlattenedBooleans again.
            {
                Params.Output.First().ExpireSolution(recompute: false); // manual implementation of the base method. Just for our 1st output param.
                //Phase = Phase == Phases.Open ? Phases.Open : Phases.CloseAndUpdated;
                //wasComputing = true;
            }


            // only relevant if 2nd output exists. Which should still work in case you have the old component on your canvas with only 1 output.
            foreach (IGH_Param item in Params.Output.Skip(1))
            {
                item.ExpireSolution(recompute: false);
            }

            SingleRunFromDoubleClick = false;

        }


        /// <summary>
        /// Checks all sources of the inputs of the index
        /// </summary>
        /// <returns></returns>
        private bool GetFlattenedBooleans(int index)
        {
            IList<IGH_Param> sources = Params.Input[index].Sources;

            if (sources == null || sources.Count == 0) return Params.Input[index].Reverse;

            // added support for multiple sources input
            foreach (var source in sources)
            {

                if (source == null) return false; // unsure if this is needed

                source.CollectData();

                IGH_Structure volatileData = source.VolatileData;

                int pathCount = volatileData.PathCount;

                for (int i = 0; i < pathCount; i++)
                {
                    System.Collections.IList branch = volatileData.get_Branch(i);

                    foreach (var item in branch)
                    {

                        if (item == null) return false; // also unsure if needed

                        // parsing numbers and strings and booleans
                        switch (item)
                        {
                            case GH_Boolean b:
                                if (b.Value == false) return false;
                                break;
                            case GH_String s:
                                if (s.Value.ToLower() != "true" || s.Value != "1") return false;
                                break;
                            case GH_Number n:
                                if (n.Value <= 0) return false;
                                break;
                            case GH_Integer g:
                                if (g.Value <= 0) return false;
                                break;

                            default: return false;

                        }

                        //// May be better off with original solution, but slightly unsure how it handles nulls etc:
                        //if (GH_Convert.ToBoolean(item, out bool result, GH_Conversion.Both) && result)
                        //{
                        //    return false;
                        //}
                    }
                }
            }

            return true;

        }

        public GH_ObjectResponse OnDoubleClick(GH_Canvas sender, GH_CanvasMouseEvent e)
        {
            SingleRunFromDoubleClick = true;
            ExpireSolution(true);

            return GH_ObjectResponse.Capture;
        }

        public override void CreateAttributes()
        {
            m_attributes = new GatekeeperAttributes(this);

        }

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon => Gatekeeper.Properties.Resources.GH_Gatekeeper.ToBitmap();

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("F798609B-3A6A-4736-BD18-E59BF1F95D65"); }
        }

        
    }
}
