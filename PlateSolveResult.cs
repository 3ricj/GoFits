using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GoFits
{
    public class PlateSolveResult
    {

        public PlateSolveResult()
        {
            Success = true;
            SolveTime = DateTime.Now;
        }

        public DateTime SolveTime { get; private set; }

        //private double _orientation;

        public double Orientation { get; set; }
        /*{
            get => _orientation;
            set => _orientation;
            {
                                _orientation = //Astrometry.EuclidianModulus(value, 360);
            }
        }*/

        public double Pixscale { get; set; }

        public double Radius { get; set; }

        public double RaDeg { get; set; }
        public double DecDeg { get; set; }


        public bool Success { get; set; }

    }
}