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
//using System.Windows.Shapes;
using nom.tam.fits;
using nom.tam.image;
using nom.tam.util;
using System.IO;



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

        }
        protected string GetOutputPath(string imageFilePath)
        {
            return Path.Combine(Path.GetDirectoryName(imageFilePath), Path.GetFileNameWithoutExtension(imageFilePath)) + ".ini";
        }
        private (double RA, double DEC) ReadFitsHeader(string Filename)
        {

            Fits f = new Fits(Filename);
            
            ImageHDU h = (ImageHDU)f.ReadHDU();
            // h myHeader cards [nom.tam.util.hashed list]
            //Object[][] data = (Object[][])h.Header.GetStringValue("SIMPLE")); 
            //            if (h.Header.)
            // IMAGETYP= 'LIGHT'   
            // NAXIS1  =                 7380 /
            // NAXIS2 = 4908 
            // EXPTIME =                  6.0 / [s] Exposure duration
            // DATE-LOC= '2020-10-07T01:28:55.164' / Time of observation (local)
            //     GAIN    =                 1208 / Sensor gain
            //     XPIXSZ  =                 4.88 / [um] Pixel X axis size
            // YPIXSZ = 4.88 / [um] Pixel Y axis size
            //     SITELAT =     47.6077777777778 / [deg] Observation site latitude
            // SITELONG = -122.335 / [deg] Observation site longitude]



            double RA = h.Header.GetDoubleValue("RA");
            double DEC = h.Header.GetDoubleValue("DEC");
            return (RA, DEC);

        }
        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            (double RA, double DEC) = ReadFitsHeader("c:/temp/test.fits");
            Console.WriteLine("RA" + RA);
            Console.WriteLine("DEC" + DEC);
        }
    }
}
