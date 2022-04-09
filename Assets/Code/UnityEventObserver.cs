using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace Avangardum.ComplectEnergoTestTask
{
    public class UnityEventObserver : MonoBehaviour
    {
        public event EventHandler<UpdateArgs> FixedUpdateTriggered;

        private void FixedUpdate()
        {
            FixedUpdateTriggered?.Invoke(this, new UpdateArgs {DeltaTime = Time.fixedDeltaTime});
        }
    }
}