using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Controls;
using System.Windows.Input;

namespace CommonTools.OpeningClient.Support
{
    public class BindableBase : INotifyPropertyChanged
    {
        protected bool _isDataValid = true;
        public ICommand TextBoxGotFocusCommand { get; set; }
        public ICommand OpenUserManualCommand { get; set; }

        public event PropertyChangedEventHandler PropertyChanged = delegate { };

        public BindableBase ()
        {
            TextBoxGotFocusCommand = new RelayCommand<object> ( TextBoxGotFocusCommandInvoke );
            OpenUserManualCommand = new RelayCommand<object> ( OpenUserManualCommandInvoke );
        }

        protected virtual void SetProperty<T> ( ref T member, T val, [CallerMemberName] string propertyName = null )
        {
            if ( object.Equals ( member, val ) ) return;

            member = val;
            PropertyChanged ( this, new PropertyChangedEventArgs ( propertyName ) );
        }

        protected virtual void OnPropertyChanged ( string propertyName )
        {
            PropertyChanged ( this, new PropertyChangedEventArgs ( propertyName ) );
        }

        public virtual bool IsDataValid ()
        {
            return _isDataValid;
        }

        /// <summary>
        /// select all the text in a textbox when user switch control using TAB
        /// </summary>
        /// <param name="obj"></param>
        private void TextBoxGotFocusCommandInvoke ( object obj )
        {
            if ( obj is TextBox )
                ( obj as TextBox ).SelectAll ();
        }

        /// <summary>
        /// Open user manual videos
        /// </summary>
        /// <param name="obj"></param>
        private void OpenUserManualCommandInvoke ( object obj )
        {
            CommonUtils.Utils.OpenUserManualLink ();
        }
    }
}