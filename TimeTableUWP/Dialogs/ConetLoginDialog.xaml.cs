﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Content Dialog item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace TimeTableUWP.Dialogs
{
    public sealed partial class ConetLoginDialog : ContentDialog
    {
        public ConetLoginDialog()
        {
            this.InitializeComponent();
        }

        private void ContentDialog_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
        }

        private void ContentDialog_SecondaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
        }

        private void keyBox1_TextChanged(object sender, TextChangedEventArgs e)
        {

        }

        private void keyBox2_TextChanged(object sender, TextChangedEventArgs e)
        {

        }

        private void keyBox3_TextChanged(object sender, TextChangedEventArgs e)
        {

        }
    }
}
