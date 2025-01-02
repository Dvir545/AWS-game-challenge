using System;
using Utils;

namespace Enemies.Chicken
{
    // chicken are only visible (have warning messages) at night
    public class ChickenWarnableBehaviour: EnemyWarnableBehaviour
    {
        private bool _needsToShow;
        private bool _isNight;

        public override void Reset()
        {
            _needsToShow = false;
            _isNight = false;
        }
        
        public override void ShowWarningSign()
        {
            if (!_isNight)
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
            return base.IsVisible() || !_isNight;  // we want the chicken to be considered visible in day
        }

        private void Start()
        {
            EventManager.Instance.StartListening(EventManager.NightStarted, RevealWarningSign);
            EventManager.Instance.StartListening(EventManager.NightEnded, (object arg0) => _isNight = false);
        }

        private void RevealWarningSign(object arg0)
        {
            _isNight = true;
            if (_needsToShow)
            {
                base.ShowWarningSign();
                _needsToShow = false;
            }
        }
    }
}