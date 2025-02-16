public class BetterFPSDisplay : FPSDisplay
{
        protected override void OnGUI()
        {
                if (!UIManager.HasInstance)
                {
                        base.OnGUI();
                }
                else if(!UIManager.Instance.IsOpen())
                {
                        base.OnGUI(); 
                }
        }
}
