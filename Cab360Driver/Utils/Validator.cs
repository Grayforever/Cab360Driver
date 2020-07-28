using AndroidX.AppCompat.Widget;
using Java.Lang;
using static Android.Widget.AutoCompleteTextView;

namespace Cab360Driver.Utils
{
    public sealed class Validator : Java.Lang.Object, IValidator
    {
        private AppCompatAutoCompleteTextView _edittext;

        public Validator(AppCompatAutoCompleteTextView edittext)
        {
            _edittext = edittext;
        }

        public ICharSequence FixTextFormatted(ICharSequence invalidText)
        {
            return null;
        }

        public bool IsValid(ICharSequence text)
        {
            var listAdapter = _edittext.Adapter;
            for (int i = 0; i < listAdapter.Count; i++)
            {
                string temp = listAdapter.GetItem(i).ToString();
                if (text.ToString().CompareTo(temp) != 0)
                    continue;
                return true;
            }
            return false;
        }
    }
}