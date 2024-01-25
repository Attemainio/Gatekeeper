using Grasshopper.Kernel;
using System;
using System.Drawing;

namespace Gatekeeper
{
    public class GatekeeperInfo : GH_AssemblyInfo
    {
        public override string Name => "Gatekeeper";

        //Return a 24x24 pixel bitmap to represent this GHA library.
        public override Bitmap Icon => null;

        //Return a short string describing the purpose of this GHA library.
        public override string Description => "";

        public override Guid Id => new Guid("1212c74b-9967-4d3b-92b7-48896b76b6e0");

        //Return a string identifying you or your company.
        public override string AuthorName => "Atte Harrikari";

        //Return a string representing your preferred contact details.
        public override string AuthorContact => "atte.harrikari@gmail.com";
    }
}