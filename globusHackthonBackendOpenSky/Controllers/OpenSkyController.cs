using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

using globusHackthonBackendOpenSky.Models;
using System.Net.Http;
using System.Text.Json;
using System.Linq;

namespace globusHackthonBackendOpenSky.Controllers
{
    public record OpenSkyState {
        private string icao24 { get; } // Unique ICAO 24-bit address of the transponder in hex string representation.
        private string callsign { get; } // Callsign of the vehicle (8 chars). Can be null if no callsign has been received.
        private string origin_country { get; } // Country name inferred from the ICAO 24-bit address.
        private int time_position { get; } // Unix timestamp (seconds) for the last position update. Can be null if no position report was received by OpenSky within the past 15s.
        private int last_contact { get; } // Unix timestamp (seconds) for the last update in general. This field is updated for any new, valid message received from the transponder.
        public float longitude { get; } // WGS-84 longitude in decimal degrees. Can be null.
        public float latitude { get; } // WGS-84 latitude in decimal degrees. Can be null.
        public float baro_altitude { get; } // Barometric altitude in meters. Can be null.
        private bool on_ground { get; }	// Boolean value which indicates if the position was retrieved from a surface position report.
        private float velocity { get; }	// Velocity over ground in m/s. Can be null.
        private float true_track { get; } // True track in decimal degrees clockwise from north (north=0°). Can be null.
        private float vertical_rate { get; } // Vertical rate in m/s. A positive value indicates that the airplane is climbing, a negative value indicates that it descends. Can be null.
        private int[] sensors { get; } // IDs of the receivers which contributed to this state vector. Is null if no filtering for sensor was used in the request.
        private float geo_altitude { get; } // Geometric altitude in meters. Can be null.
        private string squawk { get; } // The transponder code aka Squawk. Can be null.
        private bool spi { get; } // Whether flight status indicates special purpose indicator.
        private int position_source { get; } // Origin of this state’s position: 0 = ADS-B, 1 = ASTERIX, 2 = MLAT
    }

    public record OpenSkyResult {
        private int time;
        public OpenSkyState[] states { get; }
    }

    [ApiController]
    [Route("[controller]")]
    public class OpenSkyController : ControllerBase
    {
        private static readonly HttpClient client = new HttpClient();

        private readonly ILogger<OpenSkyController> _logger;

        public OpenSkyController(ILogger<OpenSkyController> logger)
        {
            _logger = logger;
        }

        private static async System.Threading.Tasks.Task<OpenSkyResult> GetDataFromOpenSkyAsync() {
            var planeTask = client.GetStreamAsync("https://opensky-network.org/api/states/all");
            return await JsonSerializer.DeserializeAsync<OpenSkyResult>(await planeTask);
        }

        private static IEnumerable<OpenSky> parseOpenSky(OpenSkyResult openSkyResult) {
            return openSkyResult.states.Select(state => new OpenSky {
                latitude = state.latitude,
                longitude = state.longitude,
                altitude = state.baro_altitude
            });
        }

        [HttpGet]
        public IEnumerable<OpenSky> Get()
        {
            var data = GetDataFromOpenSkyAsync();
            data.RunSynchronously();
            return parseOpenSky(data.Result);
        }
    }
}
