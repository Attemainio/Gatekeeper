using Grasshopper.Kernel;
using Grasshopper.Kernel.Data;
using Grasshopper.Kernel.Types;
using System;

namespace Gatekeeper
{
    [Obsolete]
    public class GH_GatekeeperComponent_old : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the GH_DataDamComponent class.
        /// </summary>
        public GH_GatekeeperComponent_old()
          : base("Gatekeeper_old", "GK",
              "The Gatekeeper component is a powerful tool designed to manage the flow of data within Grasshopper for Rhino. " +
              "It acts as a conditional gate that can prevent data from propagating further in a Grasshopper definition based on a boolean condition, " +
              "without triggering a recomputation of the solution. " +
              "This component is particularly useful when you need to control the data flow based on specific conditions, " +
              "and it ensures a seamless user experience by retaining the previous data state.",
              "Params", "Util")
        {
        }


        public override GH_Exposure Exposure => GH_Exposure.hidden;
        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("Data", "D", "Data", GH_ParamAccess.tree);
            pManager.AddBooleanParameter("Pass", "P", "True, if data is passed forward. Only one input here!", GH_ParamAccess.item, false);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Data", "D", "Data", GH_ParamAccess.item);
        }

        private GH_Structure<IGH_Goo> _data = new GH_Structure<IGH_Goo>();
        private bool _pass = false;

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            bool compute = false;
            DA.GetData(1, ref compute);

            DA.GetDataTree(0, out GH_Structure<IGH_Goo> dataTree);

            if (!_pass && compute)
                _data = dataTree.ShallowDuplicate();

            DA.SetDataTree(0, compute ? dataTree : _data);

            _pass = compute;
        }

        protected override void ExpireDownStreamObjects()
        {
            var sources = Params.Input[1].Sources;

            if (sources == null) return;


            // added support for multiple sources input
            foreach (var source in sources)
            {
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

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon => Gatekeeper.Properties.Resources.GH_Gatekeeper.ToBitmap();

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("F798609B-3A6A-4736-BD18-E59AF1F95D65"); }
        }
    }
}
