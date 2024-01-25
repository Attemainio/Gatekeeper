using GH_IO.Serialization;
using Grasshopper.GUI;
using Grasshopper.GUI.Canvas;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Data;
using Grasshopper.Kernel.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

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

        // if it was already on
        bool wasOn = false;

        // set a toggle in the right click menu on the component to avoid recomputing if the gate is already open.
        bool useStickyOn = false;

        DateTime IHasPhase.LastRun { get => lastRun; set => lastRun = value; }
        DateTime lastRun = DateTime.MinValue;



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
            pManager.AddBooleanParameter("Pass", "P", "List or tree as an input. Each item corresponds the data source.", GH_ParamAccess.tree, false);
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
            DA.GetDataTree(1, out GH_Structure<IGH_Goo> passTree);
            DA.GetDataTree(0, out GH_Structure<IGH_Goo> dataTree);

            if (!_pass && compute)
                _data = dataTree.ShallowDuplicate();

            DA.SetDataTree(0, compute ? dataTree : _data);

            _pass = compute;

            // I noticed that it is pretty challenging to output multi-output from a single output source.

        }

        protected override void ExpireDownStreamObjects()
        {
            GH_Structure<GH_Boolean> graftedStructure = new GH_Structure<GH_Boolean>();
            int count = 0;

            if (GetVolatileData(1, out List<IGH_Structure> passStruct))
            {
                foreach (IGH_Structure structure in passStruct)
                {
                    for (int i = 0; i < structure.PathCount; i++)
                    {
                        var branch = structure.get_Branch(i);

                        for (int j = 0; j < branch.Count; j++)
                        {
                            if (GH_Convert.ToBoolean(branch[j], out bool result, GH_Conversion.Both))
                            {
                                if (result == false) return;
                                graftedStructure.Append(new GH_Boolean(result), new GH_Path(count));
                                count++;
                            }
                            else
                            {
                                this.AddRuntimeMessage(GH_RuntimeMessageLevel.Error, $"Data conversion failed from {branch[j].GetType()} to GH_Boolean");
                                return;
                            }
                        }
                    }
                }
            }

            if (GetVolatileData(0, out List<IGH_Structure> dataStruct))
            {
                if (graftedStructure.PathCount != dataStruct.Count)
                {
                    this.AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Pass argument count differents from source count");
                    return;
                }
            }

            base.ExpireDownStreamObjects();
        }

        /// <summary>
        /// Gets the volatile data of the input
        /// </summary>
        /// <param name="index"></param>
        /// <param name="structure"></param>
        /// <returns></returns>
        private bool GetVolatileData(int index, out List<IGH_Structure> structures)
        {
            var sources = Params.Input[index].Sources;
            structures = new List<IGH_Structure>();

            if (sources?.Count != null)
            {
                foreach (var source in sources)
                {
                    source.CollectData();
                    structures.Add(source.VolatileData);
                }

                return true;
            }

            return false;
        }

        protected override void AppendAdditionalComponentMenuItems(ToolStripDropDown menu)
        {
            ToolStripMenuItem showRooms = Menu_AppendItem(menu, "Use Sticky On", (s, e) => { useStickyOn = !useStickyOn; }, true, useStickyOn);

            base.AppendAdditionalComponentMenuItems(menu);
        }

        public override bool Write(GH_IWriter writer)
        {
            writer.SetBoolean("useStickyOn", useStickyOn);
            return base.Write(writer);
        }

        public override bool Read(GH_IReader reader)
        {
            if (reader.ItemExists("useStickyOn"))
                useStickyOn = reader.GetBoolean("useStickyOn");

            return base.Read(reader);
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
