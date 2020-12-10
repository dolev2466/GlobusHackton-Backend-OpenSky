using System;

namespace globusHackthonBackendOpenSky.Models
{
    public record OpenSky
    {
        public string CallSign { get; set; }
        public double Longitude { get; set; } // WGS-84 longitude in decimal degrees. Can be null.
        public double Latitude { get; set; } // WGS-84 latitude in decimal degrees. Can be null.
        public double Altitude { get; set; } // Barometric altitude in meters. Can be null.
        public double TrueTrack { get; set; }
        public double VerticalRate { get; set; }
    }
}
