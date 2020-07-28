using System;

namespace Cab360Driver.Utils
{
    public sealed class SwipeActions : SwipeControllerActions
    {
        private Action<int> _onLeftClicked;
        private Action<int> _onRightClicked;

        public SwipeActions(Action<int> onLeftClicked, Action<int> onRightClicked)
        {
            _onLeftClicked = onLeftClicked;
            _onRightClicked = onRightClicked;
        }
        public override void OnLeftClicked(int position)
        {
            _onLeftClicked?.Invoke(position);
        }

        public override void OnRightClicked(int position)
        {
            _onRightClicked?.Invoke(position);
        }
    }
}