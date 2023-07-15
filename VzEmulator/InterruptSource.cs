using Konamiman.Z80dotNet;
using System;

namespace VzEmulator
{
    public class InterruptSource : IZ80InterruptSource, IInterruptEnableFlag
    {
        public bool IntActive { get; set; }

        public Boolean IsEnabled { get => IntActive; set => IntActive = value; }

        bool IZ80InterruptSource.IntLineIsActive => IntActive;

        byte? IZ80InterruptSource.ValueOnDataBus => 0;

        event EventHandler IZ80InterruptSource.NmiInterruptPulse
        {
            add
            {

            }

            remove
            {
            }
        }
    }
}
