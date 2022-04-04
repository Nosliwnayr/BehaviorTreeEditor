using System;
using System.Threading.Tasks;
using UnityEngine;

namespace BehaviorTree
{
    public class Delay : Leaf
    {
        public override string Description => "Delay execution for X seconds";

        [SerializeField]
        private float delaySeconds = 1;

        private async void DelaySuccess()
        {
            await Task.Delay(TimeSpan.FromSeconds(delaySeconds));
            state = State.Success;
        }

        protected override void OnStart()
        {
            DelaySuccess();
        }

        protected override void OnStop()
        {
        }

        protected override State OnUpdate()
        {
            return state;
        }
    }
}
