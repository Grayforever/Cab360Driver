using Android.Content;
using Android.Util;
using Android.Views;
using Android.Views.InputMethods;
using AndroidX.AppCompat.Widget;
using Google.Android.Material.TextField;

namespace Cab360Driver
{
    public class CustomAutoCompleteTextView : AppCompatAutoCompleteTextView
    {
        public CustomAutoCompleteTextView(Context context) : base(context)
        {

        }

        public CustomAutoCompleteTextView(Context context, IAttributeSet attrs) : base(context, attrs)
        {

        }

        public CustomAutoCompleteTextView(Context context, IAttributeSet attrs, int defStyleAttr) : base(context, attrs, defStyleAttr)
        {

        }

        public override IInputConnection OnCreateInputConnection(EditorInfo outAttrs)
        {
            IInputConnection ic = base.OnCreateInputConnection(outAttrs);
            if (ic != null && outAttrs.HintText == null)
            {
                IViewParent parent = Parent;
                if(parent is TextInputLayout layout)
                {
                    outAttrs.HintText = new Java.Lang.String(layout.Hint);
                }
            }
            return ic;
        }
    }
}