using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ChoETL;


namespace GoFits
{
    public class AnalyzeResult
    {

        public AnalyzeResult()
        {
            Success = true;
            SolveTime = DateTime.Now;
        }

        public DateTime SolveTime { get; private set; }

        //   1.33;5.4;416;-;-;7.34;1098;NC;1;6;-10;0.00;0.00;0.0;0;4.34;2020/09/11 06H50;6.15;0.00;
        //   1.37;0.6;0;-;-;?;13906;NC;1;0;-10;0.00;0.00;0.0;0;10.05;2020/10/20 04H15;6.14;0.00;
        //   FWHM; RND; background; curv; tilt; SNR; star count; Filter;bin;exposure;ccdTempC; ForQ;SQM;ext;Hygro; seeing;Collim;Date;Unknown0;??
        //    1  ; 2  ; 3         ; 4   ; 5   ; 6  ; 7         ; 8     ; 9 ; 10     ; 11     ; 12  ; 13;14 ; 15  ; 16    ; 16   ; 18 ; 19

        [ChoCSVRecordField(1, FieldName = "FWHM")]
        public double FWHM { get; set; }

        [ChoCSVRecordField(2, FieldName = "RND")]
        public double RND { get; set; }

        [ChoCSVRecordField(3, FieldName = "background")]
        public double background { get; set; }

        [ChoCSVRecordField(6, FieldName = "SNR")]
        public string SNR { get; set; }
        [ChoCSVRecordField(7, FieldName = "StarCount")]
        public double StarCount { get; set; }
        [ChoCSVRecordField(16, FieldName = "Collim")]
        public double Collim { get; set; }

        [ChoCSVRecordField(18, FieldName = "Unknown0")]
        public double Unknown0 { get; set; }

        public bool Success { get; set; }

    }
}
