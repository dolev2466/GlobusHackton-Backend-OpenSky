using System;

namespace globusHackthonBackendOpenSky.Models
{
    public record OpenSky
    {
        public string id { get; set; }
        public double longitude { get; set; } // WGS-84 longitude in decimal degrees. Can be null.
        public double latitude { get; set; } // WGS-84 latitude in decimal degrees. Can be null.
        public double altitude { get; set; } // Barometric altitude in meters. Can be null.
        public double true_track { get; set; }
    }
}
