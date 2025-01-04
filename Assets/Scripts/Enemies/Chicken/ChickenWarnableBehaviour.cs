using System;
using UI.WarningSign;
using Utils;
using World;

namespace Enemies.Chicken
{
    // chicken are only visible (have warning messages) at night
    public class ChickenWarnableBehaviour: EnemyWarnableBehaviour
    {
        private bool _needsToShow;

        public override void Reset()
        {
            _needsToShow = false;
        }
        
        public override void SetWarningSign(WarningSignBehaviour warningSign)
        {
            if (!DayNightManager.Instance.NightTime)
            {
                _needsToShow = true;
            }
            base.SetWarningSign(warningSign);
        }
        
        public override void ShowWarningSign()
        {
            if (!DayNightManager.Instance.NightTime)
            {
                _needsToShow = true;
            }
            else
            {
                base.ShowWarningSign();
            }
        }
        
        public override void HideWarningSign()
        {
            base.HideWarningSign();
            _needsToShow = false;
        }
        
        public override bool IsVisible()
        {
            return base.IsVisible() || !DayNightManager.Instance.NightTime;  // we want the chicken to be considered visible in day
        }

        private void Start()
        {
            EventManager.Instance.StartListening(EventManager.NightStarted, RevealWarningSign);
        }

        private void RevealWarningSign(object arg0)
        {
            if (_needsToShow)
            {
                base.ShowWarningSign();
                _needsToShow = false;
            }
        }
    }
}