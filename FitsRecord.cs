using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ChoETL;

namespace GoFits
{
    public class FitsRecord

    {

        [ChoCSVRecordField(1)]
        public string Filename   { get;  set;   }
        [ChoCSVRecordField(2)]
        public double SolvedRaDeg { get; set; }
        [ChoCSVRecordField(3)]
        public double SolvedDecDeg { get; set; }
        [ChoCSVRecordField(4)]
        public double SolvedOrientation { get; set; }

        [ChoCSVRecordField(5)]
        public double RequestedRaDeg { get; set; }
        [ChoCSVRecordField(6)]
        public double RequestedDecDeg { get; set; }


        //   1.33;5.4;416;-;-;7.34;1098;NC;1;6;-10;0.00;0.00;0.0;0;4.34;2020/09/11 06H50;6.15;0.00;
        //   FWHM; RND; background; curv; tilt; SNR; star count; Filter;bin;exposure;ccdTempC; ForQ;SQM;ext;Hygro; seeing;Collim;Date;Unknown0;??

        [ChoCSVRecordField(7)]

        public double FWHM { get; set; }


        [ChoCSVRecordField(8)]
        public double RND { get; set; }


        [ChoCSVRecordField(9)]
        public double background { get; set; }


        [ChoCSVRecordField(10)]
        public string SNR { get; set; }

        [ChoCSVRecordField(11)]
        public double StarCount { get; set; }

        [ChoCSVRecordField(12)]
        public double Collim { get; set; }


        [ChoCSVRecordField(13)]
        public double Unknown0 { get; set; }

        [ChoCSVRecordField(14)]
        public string ImageType { get; set; }
        [ChoCSVRecordField(15)]
        public int NAXIS1 { get; set; }
        [ChoCSVRecordField(16)]
        public int NAXIS2 { get; set; }


        [ChoCSVRecordField(17)]
        public DateTime LocalDate { get; set; }
        [ChoCSVRecordField(18)]
        public DateTime UTCDate { get; set; }

        [ChoCSVRecordField(19)]
        public float PixelPitch { get; set; }

        [ChoCSVRecordField(20)]
        public double SiteLat { get; set; }
        [ChoCSVRecordField(21)]
        public double SiteLong { get; set; }
        [ChoCSVRecordField(22)]
        public int Gain { get; set; }
        [ChoCSVRecordField(23)]
        public float Exposure { get; set; }

        [ChoCSVRecordField(24)]
        public double AltCalculated { get; set; }
        [ChoCSVRecordField(25)]
        public double AzCalculated { get; set; }
        [ChoCSVRecordField(26)]
        public string Object { get; set; }
        [ChoCSVRecordField(27)]
        public float SensorTempC { get; set; }

        [ChoCSVRecordField(28)]
        public float FocalLength { get; set; }

        /*public override string ToString()
            {
                return "\"{0}.\",{1}.,{2}.,{3}.,{4}.,{5}.,{6}.".FormatString(Filename, SolvedRaDeg, SolvedDecDeg, SolvedOrientation, RequestedRaDeg, RequestedDecDeg);
            } */
    }
}
