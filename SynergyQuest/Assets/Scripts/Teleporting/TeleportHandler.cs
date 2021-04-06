using UnityEngine;

namespace Teleporting
{
    /**
     * The <see cref="Teleport"/> behaviour automatically manipulates the visibility of objects
     * (eg. applying the Teleport shader and/or enabling/disabling renderers for visibility)
     *
     * If you want more control on what happens when a teleport completes implement this behaviour and subscribe to the
     * events.
     */
    public class TeleportHandler : MonoBehaviour
    {
        public delegate void TeleportOutAction();
        /**
         * Invoked by <see cref="Teleport"/> when an object begins being teleported out
         */
        public event TeleportOutAction OnTeleportOut;
        
        public delegate void TeleportInAction();
        /**
         * Invoked by <see cref="Teleport"/> when an object begins being teleported in
         */
        public event TeleportInAction OnTeleportIn;

        public void TriggerTeleportOut()
        {
            OnTeleportOut?.Invoke();
        }
        
        public void TriggerTeleportIn()
        {
            OnTeleportIn?.Invoke();
        }
    }
}