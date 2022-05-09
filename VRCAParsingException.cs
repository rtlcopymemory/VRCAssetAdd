using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.VRCAssetAdd
{
    internal class VRCAParsingException : Exception
    {
        public VRCAParsingException(string message) : base(message) { }
    }
}
