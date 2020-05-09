using Android.Runtime;
using Android.Text;
using Android.Views;
using Android.Widget;
using AndroidX.AppCompat.Widget;
using Cab360Driver.EnumsConstants;
using Google.Android.Material.TextField;
using Java.Lang;
using static Android.Views.View;

namespace Cab360Driver.Utils
{
    public class TextfieldEmptyValidator : Object, IOnKeyListener
    {
        AppCompatAutoCompleteTextView mTextView;
        TextInputLayout inputLayout;
        StringValidType mType;
        string text;
        int id1, id2;

        public TextfieldEmptyValidator(Java.Lang.Object view, StringValidType type, ViewType viewType)
        {
            mType = type;
            if(viewType == ViewType.Spinner)
            {
                mTextView = view.JavaCast<AppCompatAutoCompleteTextView>();
                id1 = mTextView.Id;
                mTextView.AfterTextChanged += MTextView_AfterTextChanged;
                text = mTextView.Text;
            }
            else
            {
                inputLayout = view.JavaCast<TextInputLayout>();
                id2 = inputLayout.Id;
                inputLayout.EditText.AfterTextChanged += EditText_AfterTextChanged;
                text = inputLayout.EditText.Text;
            }
            
        }

        private void MTextView_AfterTextChanged(object sender, AfterTextChangedEventArgs e)
        {
            text = mTextView.Text;
            CheckEmptyOrValid(text);
        }

        private void EditText_AfterTextChanged(object sender, AfterTextChangedEventArgs e)
        {
            text = inputLayout.EditText.Text;
            CheckEmptyOrValid(text);
        }

        public bool OnKey(View v, [GeneratedEnum] Keycode keyCode, KeyEvent e)
        {
            var id = v.Id;
            
            var action = e.Action;
            if (action == KeyEventActions.Up)
            {
                if (id == id1)
                {
                    CheckEmptyOrValid(mTextView.Text);
                }
                else
                {
                    CheckEmptyOrValid(inputLayout.EditText.Text);
                }
                    
            }
            return false;
        }

        private bool CheckEmptyOrValid(string text)
        {
            bool valid = false;
            switch (mType)
            {
                case StringValidType.Email:
                    valid = Android.Util.Patterns.EmailAddress.Matcher(text).Matches();
                    break;

                case StringValidType.Name:
                    valid = text.Length >= 3;
                    break;

                case StringValidType.Password:
                    valid = text.Length >= 8;
                    break;

                case StringValidType.Phone:
                    valid = text.Length >= 8;
                    break;

                case StringValidType.Place:
                    valid = text.Length >= 2;
                    break;
            }
            return valid;
        }
    }
}