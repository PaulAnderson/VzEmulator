using Konamiman.Z80dotNet;
using System;

namespace VzEmulator
{
    class InterruptSource : IZ80InterruptSource
    {

        private bool _intActive;
        public bool IntActive {
            get {
                if (_intActive)
                {
                   // _intActive = false;
                    return true;
                }
                return false;
            }
            set { _intActive = value; }  }

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
