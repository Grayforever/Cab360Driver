using Android.Graphics;
using Android.Views;
using AndroidX.RecyclerView.Widget;
using System;
using static Android.Views.View;

namespace Cab360Driver.Utils
{
    public class SwipeControllerUtils : ItemTouchHelper.Callback
    {
        enum ButtonsState
        {
            GONE,
            LEFT_VISIBLE,
            RIGHT_VISIBLE
        }

        bool swipeBack = false;
        private ButtonsState buttonShowedState = ButtonsState.GONE;
        private RectF buttonInstance = null;
        private RecyclerView.ViewHolder currentItemViewHolder = null;
        private SwipeControllerActions _buttonsActions;
        private static float buttonWidth = 300;

        public SwipeControllerUtils(SwipeControllerActions buttonsActions)
        {
            _buttonsActions = buttonsActions;
        }

        public override int GetMovementFlags(RecyclerView p0, RecyclerView.ViewHolder p1)
        {
            return MakeMovementFlags(0, ItemTouchHelper.Left | ItemTouchHelper.Right);
        }

        public override bool OnMove(RecyclerView p0, RecyclerView.ViewHolder p1, RecyclerView.ViewHolder p2)
        {
            return false;
        }

        public override void OnSwiped(RecyclerView.ViewHolder p0, int p1)
        {

        }

        public override int ConvertToAbsoluteDirection(int flags, int layoutDirection)
        {
            if (swipeBack)
            {
                swipeBack = false;
                return 0;
            }
            return base.ConvertToAbsoluteDirection(flags, layoutDirection);
        }

        public override void OnChildDraw(Canvas c, RecyclerView recyclerView, RecyclerView.ViewHolder viewHolder, float dX, float dY, int actionState, bool isCurrentlyActive)
        {
            if (actionState == ItemTouchHelper.ActionStateSwipe)
            {
                if(buttonShowedState != ButtonsState.GONE)
                {
                    if(buttonShowedState == ButtonsState.LEFT_VISIBLE)
                    {
                        dX = Math.Max(dX, buttonWidth);
                    }
                    if (buttonShowedState == ButtonsState.RIGHT_VISIBLE)
                    {
                        dX = Math.Min(dX, -buttonWidth);
                    }
                    base.OnChildDraw(c, recyclerView, viewHolder, dX, dY, actionState, isCurrentlyActive);
                }
                else
                {
                    SetTouchListener(c, recyclerView, viewHolder, dX, dY, actionState, isCurrentlyActive);
                }
            }
            if (buttonShowedState == ButtonsState.GONE)
            {
                base.OnChildDraw(c, recyclerView, viewHolder, dX, dY, actionState, isCurrentlyActive);
            }
            currentItemViewHolder = viewHolder;
        }

        private void SetTouchListener(Canvas c, RecyclerView recyclerView, RecyclerView.ViewHolder viewHolder, float dX, float dY, int actionState, bool isCurrentlyActive)
        {
            recyclerView.SetOnTouchListener(new OnTouchListener((v, e) =>
            {
                swipeBack = e.Action == MotionEventActions.Cancel || e.Action == MotionEventActions.Up;
                if (swipeBack)
                {
                    if (dX < -buttonWidth)
                    {
                        buttonShowedState = ButtonsState.RIGHT_VISIBLE;
                    }
                    else if (dX > buttonWidth)
                    {
                        buttonShowedState = ButtonsState.LEFT_VISIBLE;
                    }

                    if (buttonShowedState != ButtonsState.GONE)
                    {
                        SetTouchDownListener(c, recyclerView, viewHolder, dX, dY, actionState, isCurrentlyActive);
                        SetItemsClickable(recyclerView, false);
                    }
                }
            }));
        }

        private void SetTouchDownListener(Canvas c, RecyclerView recyclerView, RecyclerView.ViewHolder viewHolder, float dX, float dY, int actionState, bool isCurrentlyActive)
        {
            recyclerView.SetOnTouchListener(new OnTouchListener((v, e) =>
            {
                if (e.Action == MotionEventActions.Down)
                {
                    SetTouchUpListener(c, recyclerView, viewHolder, dX, dY, actionState, isCurrentlyActive);
                }
            }));
        }

        private void SetTouchUpListener(Canvas c, RecyclerView recyclerView, RecyclerView.ViewHolder viewHolder, float dX, float dY, int actionState, bool isCurrentlyActive)
        {
            recyclerView.SetOnTouchListener(new OnTouchListener((v, e) =>
            {
                if (e.Action == MotionEventActions.Up)
                {
                    base.OnChildDraw(c, recyclerView, viewHolder, 0F, dY, actionState, isCurrentlyActive);
                    recyclerView.SetOnTouchListener(new OnTouchListener((v, e) =>
                    {

                    }));

                }
                SetItemsClickable(recyclerView, true);
                swipeBack = false;

                if(_buttonsActions != null && buttonInstance != null && buttonInstance.Contains(e.GetX(), e.GetY()))
                {
                    if(buttonShowedState == ButtonsState.LEFT_VISIBLE)
                    {
                        _buttonsActions.OnLeftClicked(viewHolder.AdapterPosition);
                    }
                    else if(buttonShowedState == ButtonsState.RIGHT_VISIBLE)
                    {
                        _buttonsActions.OnRightClicked(viewHolder.AdapterPosition);
                    }
                }
                buttonShowedState = ButtonsState.GONE;
                currentItemViewHolder = null;
            })); 
        }

        private void SetItemsClickable(RecyclerView recyclerView, bool v)
        {
            for (int i = 0; i < recyclerView.ChildCount; ++i)
            {
                recyclerView.GetChildAt(i).Clickable = v;
            }
        }

        private void DrawButtons(Canvas c, RecyclerView.ViewHolder viewHolder)
        {
            float buttonWidthWithoutPadding = buttonWidth - 20;
            float corners = 16;

            View itemView = viewHolder.ItemView;
            Paint p = new Paint();

            RectF leftButton = new RectF(itemView.Left, itemView.Top, itemView.Left + buttonWidthWithoutPadding, itemView.Bottom);
            p.Color = Color.ParseColor("#673ab7");
            c.DrawRoundRect(leftButton, corners, corners, p);
            DrawText("EDIT", c, leftButton, p);

            RectF rightButton = new RectF(itemView.Right - buttonWidthWithoutPadding, itemView.Top, itemView.Right, itemView.Bottom);
            p.Color = Color.ParseColor("#ff4651");
            c.DrawRoundRect(rightButton, corners, corners, p);
            DrawText("DELETE", c, rightButton, p);

            buttonInstance = null;
            if (buttonShowedState == ButtonsState.LEFT_VISIBLE)
            {
                buttonInstance = leftButton;
            }
            else if (buttonShowedState == ButtonsState.RIGHT_VISIBLE)
            {
                buttonInstance = rightButton;
            }
        }

        private void DrawText(String text, Canvas c, RectF button, Paint p)
        {
            float textSize = 60;
            p.Color = Color.White;
            p.AntiAlias = true;
            p.TextSize = textSize;

            float textWidth = p.MeasureText(text);
            c.DrawText(text, button.CenterX() - (textWidth / 2), button.CenterY() + (textSize / 2), p);
        }

        public void OnDraw(Canvas c)
        {
            if(currentItemViewHolder != null)
            {
                DrawButtons(c, currentItemViewHolder);
            }
        }

        internal sealed class OnTouchListener : Java.Lang.Object, IOnTouchListener
        {
            private Action<View, MotionEvent> _onTouch;
            public OnTouchListener(Action<View, MotionEvent> onTouch)
            {
                _onTouch = onTouch;
            }
            public bool OnTouch(View v, MotionEvent e)
            {
                _onTouch?.Invoke(v, e);
                return false;
            }
        }
    }
}