using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace VzEmulator
{
    public interface IMachineView
    {
        Control RenderControl { get; }
        void UpdateMCStart(ushort value);
        void UpdateMcEnd(ushort value);
        void UpdateStats(MachineStats stats);
        void RenderComplete();
    }
}
