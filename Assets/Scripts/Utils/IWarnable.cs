using UI.WarningSign;

namespace Utils
{
    public interface IWarnable  // triggers warning sign if destroyed
    {
        public void SetWarningSign(WarningSignBehaviour warningSign);
        public bool IsVisible();
    }
}