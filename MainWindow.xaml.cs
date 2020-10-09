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
using System.CodeDom.Compiler;

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
            double RA;
            double DEC; 

            try
            {
                Fits f = new Fits(Filename);

                ImageHDU h = (ImageHDU)f.ReadHDU();

                //other things we might want: 
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

                RA = h.Header.GetDoubleValue("RA") / 15;
                DEC = h.Header.GetDoubleValue("DEC");
                f.Close();
                
            } catch { Console.WriteLine("Error opening fits..");  return (-1, -1);  }

            return (RA, DEC);

        }
        private string GetArguments(string imageFilePath, double RA, double DEC)
        {
            var args = new List<string>();
            args.Add($"-f \"{imageFilePath}\"");
            //args.Add($"-z 4");
            args.Add($"-s 500");
            if (RA != -1)
            {
                args.Add($"-r 30"); // search radius
                args.Add($"-ra {RA.ToString()}"); // in degrees. 
                var spd = Math.Round(DEC + 90.0, 6);
                args.Add($"-spd {spd.ToString()}");

            } else {
                //Search field radius
                args.Add($"-r {180}");
            }
        
        return string.Join(" ", args);

        }
        private void ExecuteAstap( string args)
        {
            string executableLocation = "c:\\Program Files\\astap\\astap.exe"; 

            System.Diagnostics.Process process = new System.Diagnostics.Process();
            System.Diagnostics.ProcessStartInfo startInfo = new System.Diagnostics.ProcessStartInfo();

            startInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Normal;
            startInfo.FileName = executableLocation;
            startInfo.UseShellExecute = false;
            startInfo.RedirectStandardOutput = true;
            startInfo.CreateNoWindow = true;
            startInfo.Arguments = args;
            process.StartInfo = startInfo;
            process.EnableRaisingEvents = true;
            process.Start();
            string result = process.StandardOutput.ReadToEnd();
            

        }
        private PlateSolveResult ReadAstapOutput(string outputfile)
        {
            var result = new PlateSolveResult() { Success = false };
            var dict = File.ReadLines(outputfile)
                .Where(line => !string.IsNullOrWhiteSpace(line))
                .Select(line => line.Split(new char[] { '=' }, 2, 0))
                .ToDictionary(parts => parts[0], parts => parts[1]);

            dict.TryGetValue("WARNING", out var warning);

            result.Ra = double.Parse(dict["CRVAL1"]) / 15;

           result.Dec = double.Parse(dict["CRVAL2"]);
            result.Orientation = double.Parse(dict["CROTA2"]);
            //result.Orientation = double.Parsedict["CRVAL2"]);


            return result;

        }
        private void Button_Click_1(object sender, RoutedEventArgs e)
        {

            string imageFilePath = "c:\\temp\\test.fits";
            string outputFilePath = GetOutputPath(imageFilePath);

            
            (double RA, double DEC) = ReadFitsHeader(imageFilePath);
            Console.WriteLine("Input Ra (hours): " + RA);
            Console.WriteLine("Input Dec (degrees):" + DEC);

            string ASTAP_args = GetArguments(imageFilePath, RA, DEC);
            PlateSolveResult results = new PlateSolveResult();

            if (File.Exists(outputFilePath)) {
                results = ReadAstapOutput(outputFilePath);
            } else {

                ExecuteAstap(ASTAP_args);

                if (!File.Exists(outputFilePath)) { Console.WriteLine("error, plate solve failed..."); } else
                {
                    results = ReadAstapOutput(outputFilePath);
                }
                
            }
            Console.WriteLine("Output Ra:  " + results.Ra);
            Console.WriteLine("Output Dec:  " + results.Dec);
            Console.WriteLine("Output orientation:  " + results.Orientation);



        }
    }
}
