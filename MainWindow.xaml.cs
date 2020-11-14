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
using ChoETL;
using System.Threading;
using System.Globalization;
using ASCOM.Astrometry.Transform;
using ASCOM.Utilities;
using System.Diagnostics;
using MetadataExtractor;
using MetadataExtractor.Formats.Exif;
using System.Text.RegularExpressions;

namespace GoFits
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        ASCOM.Astrometry.Transform.Transform transform = new ASCOM.Astrometry.Transform.Transform();
        Util utility = new ASCOM.Utilities.Util();
        ASCOM.Astrometry.AstroUtils.AstroUtils apUtil = new ASCOM.Astrometry.AstroUtils.AstroUtils();
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
        private FitsHeader  ReadFits(string Filename)
        {
/*            double RA;
            double DEC;
            float[][] imageData;
            string LocalDate;
            double SiteLong;
            double SiteLat; */
            FitsHeader fitsheader = new FitsHeader();


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

                fitsheader.ImageType = h.Header.GetStringValue("IMAGETYP");
                fitsheader.NAXIS1 = h.Header.GetIntValue("NAXIS1");
                fitsheader.NAXIS2 = h.Header.GetIntValue("NAXIS2");
                fitsheader.DecDeg = h.Header.GetDoubleValue("DEC");
                fitsheader.RaDeg = h.Header.GetDoubleValue("RA") / 15;
                fitsheader.Exposure = h.Header.GetFloatValue("EXPOSURE");


                fitsheader.LocalDate = DateTime.ParseExact(h.Header.GetStringValue("DATE-LOC"), "yyyy-MM-ddTHH:mm:ss.fff", CultureInfo.InvariantCulture);
                fitsheader.UTCDate = DateTime.ParseExact(h.Header.GetStringValue("DATE-OBS"), "yyyy-MM-ddTHH:mm:ss.fff", CultureInfo.InvariantCulture);

                fitsheader.SiteLat = h.Header.GetDoubleValue("SITELAT");
                fitsheader.SiteLong = h.Header.GetDoubleValue("SITELONG");
                fitsheader.PixelPitch = h.Header.GetFloatValue("XPIXSZ");
                fitsheader.Gain = h.Header.GetIntValue("GAIN");
                fitsheader.SensorTempC = h.Header.GetFloatValue("CCD-TEMP");
                fitsheader.FocalLength = h.Header.GetFloatValue("FOCALLEN");
                fitsheader.Object = h.Header.GetStringValue("OBJECT");


                /*RA = h.Header.GetDoubleValue("RA") / 15;
                DEC = h.Header.GetDoubleValue("DEC");
                LocalDate = h.Header.GetStringValue("DATE-LOC");
                SiteLat = h.Header.GetDoubleValue("SITELAT");
                SiteLong = h.Header.GetDoubleValue("SITELAT");


                //imageData = (float[][])h.Kernel; */

                f.Close();

            }
            catch { Console.WriteLine("Error opening fits.."); return (fitsheader); }

            return (fitsheader);

        }
        private string GetArguments(string imageFilePath, double RA, double DEC, double vfov)
        {
            var args = new List<string>();
            args.Add($"-f \"{imageFilePath}\"");
            //args.Add($"-z 4");
            args.Add($"-s 500");
            //args.Add($"-analyse");


            if (RA != -1)
            {
                args.Add($"-r 30"); // search radius
                args.Add($"-ra {RA.ToString()}"); // in degrees. 
                var spd = Math.Round(DEC + 90.0, 6);
                args.Add($"-spd {spd.ToString()}");

            }
            else
            {
                //Search field radius
                args.Add($"-r {180}");
            }
            if (vfov != -1)
            {
                args.Add($"-fov {vfov.ToString()}");
            }

            return string.Join(" ", args);

        }
        private PlateSolveResult ExecuteAstap(string Filename)
        {
            // Path.Combine(Path.GetDirectoryName(imageFilePath), Path.GetFileNameWithoutExtension(imageFilePath)) + ".ini";
            string destinationFile = Path.Combine(Path.GetDirectoryName(Filename), Path.GetFileNameWithoutExtension(Filename)) + ".ini";
            string WCSdestinationFile = Path.Combine(Path.GetDirectoryName(Filename), Path.GetFileNameWithoutExtension(Filename)) + ".wcs";
            string ASTAP_args;

            if (File.Exists(destinationFile) && File.Exists(WCSdestinationFile)) { return ReadAstapOutput(destinationFile); }

            double vfov = -1;

            if (Path.GetExtension(Filename).ToUpper() == ".CR2" || Path.GetExtension(Filename).ToUpper() == ".CR3" || Path.GetExtension(Filename).ToUpper() == ".TIF")
            {
                IEnumerable<MetadataExtractor.Directory> directories = ImageMetadataReader.ReadMetadata(Filename);
                var subIfdDirectory = directories.OfType<ExifSubIfdDirectory>().FirstOrDefault();
                //                var dateTime = subIfdDirectory?.GetDescription(ExifDirectoryBase.TagDateTime);
                try
                {
                    double focalLen = Convert.ToDouble(Regex.Replace(subIfdDirectory?.GetDescription(ExifDirectoryBase.TagFocalLength), @" mm", ""));
                    if (focalLen != 0) { vfov = 2 * Math.Atan(12 / focalLen); }   //  12 is half the height of the 24mm tall sensor 
                    vfov = (180 / Math.PI) * vfov;
                }
                catch { Console.WriteLine(" Error reading exif from file... "); }

            }
//            else if ( Path.GetExtension(Filename) == ".fits" || Path.GetExtension(Filename) == ".fit" ) {
 //               ASTAP_args = GetArguments(Filename, -1, -1, -1);
 //           }

            ASTAP_args = GetArguments(Filename, -1, -1, vfov);

            Console.WriteLine("plate solve args: " + ASTAP_args);


            string executableLocation = @"c:\Program Files\astap\astap.exe";

            System.Diagnostics.Process process = new System.Diagnostics.Process();
            System.Diagnostics.ProcessStartInfo startInfo = new System.Diagnostics.ProcessStartInfo();

            startInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Normal;
            startInfo.FileName = executableLocation;
            startInfo.UseShellExecute = false;
            startInfo.RedirectStandardOutput = true;
            startInfo.CreateNoWindow = true;
            startInfo.Arguments = ASTAP_args;
            process.StartInfo = startInfo;
            process.EnableRaisingEvents = true;
            process.Start();
            string result = process.StandardOutput.ReadToEnd();

            return ReadAstapOutput(destinationFile);

        }
        private PlateSolveResult ReadAstapOutput(string outputfile)
        {
            var result = new PlateSolveResult() { Success = false };
            var dict = File.ReadLines(outputfile)
                .Where(line => !string.IsNullOrWhiteSpace(line))
                .Select(line => line.Split(new char[] { '=' }, 2, 0))
                .ToDictionary(parts => parts[0], parts => parts[1]);

            //dict.TryGetValue("PLTSOLVD", out var solve_result);
            if (Convert.ToString(dict["PLTSOLVD"]) == "F") { return result; }

            dict.TryGetValue("WARNING", out var warning);

            result.RaDeg = double.Parse(dict["CRVAL1"]) / 15;

            result.DecDeg = double.Parse(dict["CRVAL2"]);
            result.Orientation = double.Parse(dict["CROTA2"]);
            //result.Orientation = double.Parsedict["CRVAL2"]);


            return result;

        }
        private void MakeThumbNails(string Filename, int ImageHeight, int ImageWidth)
        {

            string CenterThumb = Path.Combine(Path.GetDirectoryName(Filename), Path.GetFileNameWithoutExtension(Filename)) + "-centerthumb.jpg";
            string CornerThumb= Path.Combine(Path.GetDirectoryName(Filename), Path.GetFileNameWithoutExtension(Filename)) + "-cornerthumb.jpg";
            string Thumb = Path.Combine(Path.GetDirectoryName(Filename), Path.GetFileNameWithoutExtension(Filename)) + "-thumb.jpg";

            ProcessStartInfo start = new ProcessStartInfo();
            System.Diagnostics.Process p;
            start.FileName = @"c:\Program Files\IrfanView\i_view64.exe"; // note this needs to have the plugins installed in addition to the executable.  It needs to be able to open fits files to work.
            start.WindowStyle = ProcessWindowStyle.Hidden;
            start.CreateNoWindow = true;


            if (!File.Exists(Thumb)) {
                start.Arguments = Filename + "/convert=" + Thumb;
                p = Process.Start(start);
                p.WaitForExit();
            }
            if (!File.Exists(CenterThumb)) {
                int Xpos = (int)(((double)ImageHeight / 2) - 100);
                int Ypos = (int)(((double)ImageWidth / 2) - 100);
                                start.Arguments = Filename + "/crop=(" + Xpos +  "," + Ypos + ",200,200) /resize=(600,600) /convert=" + CenterThumb;
                p = Process.Start(start);
                p.WaitForExit();
            }
            if (!File.Exists(CornerThumb)) {
                start.Arguments = Filename + "/crop=(0,0,200,200) /resize=(600,600) /convert=" + CornerThumb;
                p = Process.Start(start);
                p.WaitForExit();
            }

            /*  
            /crop=(2918,1918,200,200) /resize=(600,600) /contrast=100 /convert="J:\temp\Analyze_test\Panel 5_6.00s_200g_0001-CENTERTHUMB.jpg"
            /crop=(0,0,200,200) /resize=(600,600) /contrast=100 /convert="J:\temp\Analyze_test\Panel 5_6.00s_200g_0001-CORNERTHUMB.jpg"
            /contrast=100 /convert="J:\temp\Analyze_test\Panel 5_6.00s_200g_0001-THUMB.jpg"
            */
        }
        private void ExecuteAnalyze(string Filename)
        {

            string sourceFile = Path.Combine(@"C:\MaxPilote\Temp\", Path.GetFileNameWithoutExtension(Filename)) + ".csv";
            string destinationFile = Path.Combine(Path.GetDirectoryName(Filename), Path.GetFileNameWithoutExtension(Filename)) + ".csv";

            if (File.Exists(destinationFile)) { return; }

            string cwd = Path.Combine(Path.GetTempPath(), "MaxPilote-Thread" + Thread.CurrentThread.ManagedThreadId.ToString());
            if (!System.IO.Directory.Exists(cwd)) { System.IO.Directory.CreateDirectory(cwd); }

            // Max Pilote doesn't read CR2 files.. maybe TIF? 
            if (Path.GetExtension(Filename).ToUpper() == ".CR2" || Path.GetExtension(Filename).ToUpper() == ".CR3")
            {


            }

            //Console.WriteLine(OutputFileName);

            string[] lines = { Filename, "0", "0", "1", "0", "1.65535714285714", "400", "0", "", "" };
            string StarsFile = Path.Combine(cwd, @"Ananlyse_Stars.txt");


            ProcessStartInfo start = new ProcessStartInfo();
            start.WorkingDirectory = cwd;

            start.FileName = @"C:\MaxPilote\Ananlyse_Stars.exe";
            start.WindowStyle = ProcessWindowStyle.Hidden;
            start.CreateNoWindow = true;

            System.Diagnostics.Process p;

            try { System.IO.File.WriteAllLines(StarsFile, lines); } catch { Console.WriteLine("Error writing stars file"); }
            p = Process.Start(start);

            p.WaitForExit();

            if (p.ExitCode != 0) { Console.WriteLine("Error executing: " + p.ExitCode + " File: " + Filename); }

            //Thread.Sleep(100);



            if (File.Exists(sourceFile))
            {
                try
                {
                    System.IO.File.Move(sourceFile, destinationFile);
                }
                catch
                {
                    Console.WriteLine("Error moving file.. " + destinationFile);
                }
            }
            else
            {
                Console.WriteLine("Error no output file found: " + sourceFile);
            }

            // cleanup

            if (File.Exists(sourceFile)) { File.Delete(StarsFile); }
            if (System.IO.Directory.Exists(cwd)) { System.IO.Directory.Delete(cwd); }


        }
        public (double Alt, double Az) ConvertAltAz(double Lat, double Long, double RaDeg, double DecDeg, DateTime Date)
        {


            transform.SiteLatitude = Lat;
            transform.SiteLongitude = Long;
            transform.SiteElevation = 0;
            transform.SiteTemperature = 20.0;
            transform.JulianDateTT = utility.DateLocalToJulian(Date);


            transform.SetJ2000(RaDeg /15, DecDeg); //  This takes RA as hours, thus the dvidie by 15.  https://ascom-standards.org/Help/Developer/html/M_ASCOM_Astrometry_Transform_Transform_SetJ2000.htm
            //Console.WriteLine("Azimuth: " + transform.AzimuthTopocentric.ToString() + " Elevation: " + transform.ElevationTopocentric.ToString());

            return (transform.ElevationTopocentric, transform.AzimuthTopocentric);

        }
        private void Button_Click_1(object sender, RoutedEventArgs e)
        {

            string baseDirectory = InputFileTextBox.Text;

            IEnumerable<string> filesToSolve = Enumerable.Empty<string>();
            try
            {
                filesToSolve = System.IO.Directory.EnumerateFiles(baseDirectory, "*.fits", SearchOption.AllDirectories)
                    .Union(System.IO.Directory.EnumerateFiles(baseDirectory, "*.cr2", SearchOption.AllDirectories))
                    .Union(System.IO.Directory.EnumerateFiles(baseDirectory, "*.tif", SearchOption.AllDirectories));
                /*var directory = new DirectoryInfo(baseDirectory);
                var masks = new[] { "*.cr2", "*.fits" };
               filesToSolve =  masks.SelectMany(directory.EnumerateFiles); */
            }
            catch (IOException ex)
            {
                Console.WriteLine("Error enumerating path" + ex);
            }

            string outputcsv_log = Path.Combine(baseDirectory, "gofits-4.csv"); 
            var csvWriter = new ChoCSVWriter<FitsRecord>(outputcsv_log).WithFirstLineHeader();
            object WriteLock = new object();

            
            ParallelOptions options = new ParallelOptions { MaxDegreeOfParallelism = 8 };
            //foreach (var imageFilePath in filesToSolve)
            Parallel.ForEach(filesToSolve, options, imageFilePath =>
            {

                try
                {

                    //string outputFilePath = GetOutputPath(imageFilePath);
                    Console.Write(imageFilePath);
                    FitsHeader fitsheader = new FitsHeader();

                    if (Path.GetExtension(imageFilePath) == "fits") { 
                        fitsheader = ReadFits(imageFilePath);
                    }
                    Console.Write(",Input Ra (degrees): " + fitsheader.RaDeg);
                    Console.Write(",Input Dec (degrees):" + fitsheader.DecDeg);


                    PlateSolveResult platesolveresults = new PlateSolveResult();
                    //platesolveresults = ExecuteAstap(imageFilePath);

                    
                    Console.Write(",Output Ra:  " + platesolveresults.RaDeg);
                    Console.Write(",Output Dec:  " + platesolveresults.DecDeg);
                    Console.WriteLine(",Output orientation:  " + platesolveresults.Orientation);

                    string AnalyzeFilename = Path.Combine(Path.GetDirectoryName(imageFilePath), Path.GetFileNameWithoutExtension(imageFilePath)) + ".csv";
                    ExecuteAnalyze(imageFilePath);

                    MakeThumbNails(imageFilePath, fitsheader.NAXIS1, fitsheader.NAXIS2);

                    lock (WriteLock)
                    {
                        FitsRecord fitsrecord = new FitsRecord();
                        fitsrecord.Filename = imageFilePath;
                        fitsrecord.RequestedDecDeg = fitsheader.DecDeg;
                        fitsrecord.RequestedRaDeg = fitsheader.RaDeg;
                        fitsrecord.SolvedDecDeg = platesolveresults.DecDeg;
                        fitsrecord.SolvedRaDeg = platesolveresults.RaDeg;
                        fitsrecord.SolvedOrientation = platesolveresults.Orientation;


                        fitsrecord.ImageType = fitsheader.ImageType;
                        fitsrecord.NAXIS1 = fitsheader.NAXIS1;
                        fitsrecord.NAXIS2 = fitsheader.NAXIS2;
                        fitsrecord.Exposure = fitsheader.Exposure;

                        fitsrecord.LocalDate = fitsheader.LocalDate;
                        fitsrecord.UTCDate = fitsheader.UTCDate;

                        fitsrecord.SiteLat = fitsheader.SiteLat;
                        fitsrecord.SiteLong = fitsheader.SiteLong;
                        fitsrecord.PixelPitch = fitsheader.PixelPitch;
                        fitsrecord.Gain = fitsheader.Gain;
                        fitsrecord.SensorTempC = fitsheader.SensorTempC;
                        fitsrecord.FocalLength = fitsheader.FocalLength;
                        fitsrecord.Object = fitsheader.Object;

                        //                        public (double Alt, double Az) ConvertAltAz(double Lat, double Long, double RaHours, double DecHours, DateTime Date)

                        (fitsrecord.AltCalculated, fitsrecord.AzCalculated) = ConvertAltAz(fitsheader.SiteLat, fitsheader.SiteLong, fitsheader.RaDeg, fitsheader.DecDeg, fitsheader.LocalDate);
                        //    (fitsheader.LocalDate;);


                        if (File.Exists(AnalyzeFilename))
                        {
                            foreach (var rec in new ChoCSVReader<AnalyzeResult>(AnalyzeFilename).WithDelimiter(";"))
                            {
                                fitsrecord.FWHM = rec.FWHM;
                                fitsrecord.RND = rec.RND;
                                fitsrecord.background = rec.background;
                                fitsrecord.SNR = rec.SNR;
                                fitsrecord.StarCount = rec.StarCount;
                                fitsrecord.Collim = rec.Collim;
                                fitsrecord.Unknown0 = rec.Unknown0;

                            }
                        }

                        csvWriter.Write(fitsrecord);
                        

                    }



                }
                catch (IOException ex)
                {
                    Console.WriteLine("Error accessing file" + ex);
                }




            }
            );

            csvWriter.Close();
            Console.WriteLine("Completed..."); 

            /*

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
                */


        }

    }
}
