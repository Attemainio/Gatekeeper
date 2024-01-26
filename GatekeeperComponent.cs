using Grasshopper.Kernel;
using Grasshopper.Kernel.Data;
using Grasshopper.Kernel.Types;
using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace Gatekeeper
{
    public class GH_GatekeeperComponent : GH_Component
    {
        /// <summary>
        /// used for the cached data. this is needed if we reconnect the output from this component to new parameters.
        /// </summary>
        private GH_Structure<IGH_Goo> _data = new GH_Structure<IGH_Goo>();
        private bool _pass = false;
        private bool _forcePass = false;
        private bool updateOutput = false;

        /// <summary>
        /// Initializes a new instance of the GH_DataDamComponent class.
        /// </summary>
        public GH_GatekeeperComponent()
          : base("Gatekeeper", "GK",
              "The Gatekeeper component is a powerful tool designed to manage the flow of data within Grasshopper for Rhino. " +
              "It acts as a conditional gate that can prevent data from propagating further in a Grasshopper definition based on a boolean condition, " +
              "without triggering a recomputation of the solution. " +
              "This component is particularly useful when you need to control the data flow based on specific conditions, " +
              "and it ensures a seamless user experience by retaining the previous data state.",
              "Params", "Util")
        {
        }

        public override void CreateAttributes()
        {
            m_attributes = new GatekeeperAttributes(this);
        }

        private static Bitmap RedIcon => Gatekeeper.Properties.Resources.GH_Gatekeeper_Red.ToBitmap();
        private static Bitmap GreenIcon => Gatekeeper.Properties.Resources.GH_Gatekeeper_Green.ToBitmap();

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("Data", "D", "Data", GH_ParamAccess.tree);
            pManager.AddBooleanParameter("Pass", "P", "List or tree as an input. Each item corresponds the data source. Only data item as an input.", GH_ParamAccess.tree, false);
            pManager[0].Optional = true;
            pManager[1].Optional = true;
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Data", "D", "Data", GH_ParamAccess.tree);
        }

        protected override void AppendAdditionalComponentMenuItems(ToolStripDropDown menu)
        {
            ToolStripMenuItem showRooms = Menu_AppendItem(menu, "Update Output", UpdateOutput_Click, true, updateOutput);
            base.AppendAdditionalComponentMenuItems(menu);
        }

        private void UpdateOutput_Click(object sender, EventArgs e)
        {
            updateOutput = !updateOutput;
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            bool compute;

            if (_forcePass)
                compute = true;
            else
            {
                DA.GetDataTree(1, out GH_Structure<GH_Boolean> computeTree);

                if (computeTree?.DataCount != 1)
                {
                    this.AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Sorry, one data item for pass only!");
                    return;
                }

                compute = computeTree.get_FirstItem(false).Value;
            }

            SetIconOverride(compute ? GreenIcon : RedIcon);

            if (compute)
            {
                DateTime time = DateTime.Now;
                this.Message = $"Last update: {time.ToString("HH:mm:ss")}";
            }

            DA.GetDataTree(0, out GH_Structure<IGH_Goo> dataTree);

            if (!_pass && compute) _data = dataTree.ShallowDuplicate();

            DA.SetDataTree(0, compute ? dataTree : _data);

            _pass = compute;
            _forcePass = false;
        }

        protected override void ExpireDownStreamObjects()
        {
            if (_forcePass || updateOutput)
            {
                base.ExpireDownStreamObjects();
                return;
            }

            var sources = Params.Input[1].Sources;

            if (sources?.Count > 0)
            {
                var source = sources[0];
                source.CollectData();

                var volatileData = source.VolatileData;

                if (!volatileData.IsEmpty && volatileData.get_Branch(0)?.Count > 0)
                {
                    var dataItem = volatileData.get_Branch(0)[0];

                    if (GH_Convert.ToBoolean(dataItem, out bool result, GH_Conversion.Both) && result)
                    {
                        base.ExpireDownStreamObjects();
                    }
                }
            }
        }

        public GH_ObjectResponse OnDoubleClick(GH_Canvas _1, GH_CanvasMouseEvent _2)
        {
            _forcePass = true;  
            ExpireSolution(true);

            return GH_ObjectResponse.Capture;
        }

        public override bool Write(GH_IWriter writer)
        {
            writer.SetBoolean("UpdateOutput", updateOutput);
            return base.Write(writer);
        }

        public override bool Read(GH_IReader reader)
        {
            if (reader.ItemExists("UpdateOutput"))
                updateOutput = reader.GetBoolean("UpdateOutput");

            return base.Read(reader);
        }

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon => RedIcon;

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("BFC6B1C3-56E1-4AD8-B14D-48044403B52E"); }
        }
    }
}
