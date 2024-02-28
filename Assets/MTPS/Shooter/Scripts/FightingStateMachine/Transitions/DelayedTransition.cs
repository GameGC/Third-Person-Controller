using System;
using System.Timers;
using GameGC.CommonEditorUtils.Attributes;
using MTPS.Core;
using MTPS.Core.CodeStateMachine;
using UnityEngine;

namespace MTPS.Shooter.FightingStateMachine.Transitions
{
    [Serializable]
    public class DelayedTransition : BaseStateTransition
    {
        [SerializeField,ClipToSeconds] private float delayTime;

        private bool _wasStarted;
        private bool _wasTimeFinished;

        private Timer _tempTimer;
    
        public override void Initialise(IStateMachineVariables variables, IReferenceResolver resolver)
        {
            _tempTimer = new Timer(delayTime * 1000) {AutoReset = false };
            _tempTimer.Elapsed += TimerOnElapsed;
        }

        private void TimerOnElapsed(object sender, ElapsedEventArgs e)
        {
            _wasTimeFinished = true;
            _tempTimer.Stop();
            _tempTimer.Enabled = false;
        }
    
        public override bool couldHaveTransition
        {
            get
            {
                if (!_wasStarted)
                {
                    _tempTimer.Enabled = true;
                    _tempTimer.Start();
                    _wasStarted = true;
                }

                if (_wasTimeFinished)
                {
                    _wasTimeFinished = false;
                    _wasStarted = false;
                    return true;
                }

                return false;
            }
        }
    
    
    }
}