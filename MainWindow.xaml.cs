using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using nom.tam.fits;
using nom.tam.image;
using nom.tam.util;


namespace GoFits
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new Ookii.Dialogs.Wpf.VistaFolderBrowserDialog();
            if (dialog.ShowDialog(this).GetValueOrDefault())
            {
                InputFileTextBox.Text = dialog.SelectedPath;
            }
            Fits f = new Fits("c:/temp/test.fits");
            ImageHDU h = (ImageHDU)f.ReadHDU();

            /*
            Object[][] data = (Object[][])h.getData().getData();

            for (int i = 0; i < data.length; i += 1)
            {
                int[] params = (int[])data[i][0];
                int[][] img = (int[][])data[i][1];
                ... Process a group...
    }*/
        }
    }
}
