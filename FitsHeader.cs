using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GoFits
{
    public class FitsHeader
    {

        public FitsHeader()
        {
            Success = true;
        }
        public string ImageType;
        public int NAXIS1;
        public int NAXIS2; 


        public double RaDeg;
        public double DecDeg;

        public DateTime LocalDate;
        public DateTime UTCDate;

        public float PixelPitch;

        public double SiteLat;
        public double SiteLong;
        public int Gain;
        public float Exposure;

        public double AltCalculated;
        public double AzCalculated;
        public string Object;
        public float SensorTempC;

        public float FocalLength; 


        public bool Success;

    }
}
