using System;

namespace globusHackthonBackendOpenSky.Models
{
    public record OpenSky
    {
        public float longitude { get; set; } // WGS-84 longitude in decimal degrees. Can be null.
        public float latitude { get; set; } // WGS-84 latitude in decimal degrees. Can be null.
        public float altitude { get; set; } // Barometric altitude in meters. Can be null.
    }
}
