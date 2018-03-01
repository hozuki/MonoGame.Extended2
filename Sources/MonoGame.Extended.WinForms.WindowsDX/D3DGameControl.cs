namespace MonoGame.Extended.WinForms.WindowsDX {
    public class D3DGameControl : GameControl {

        public D3DGameControl() {
            WindowBackend = new D3DWindowBackend();
        }

    }
}
