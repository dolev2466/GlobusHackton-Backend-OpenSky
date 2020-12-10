using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

using globusHackthonBackendOpenSky.Models;
using System.Net.Http;
using System.Text.Json;
using System.Linq;
using System.Threading.Tasks;

namespace globusHackthonBackendOpenSky.Controllers
{
    enum OpenSkyFields {
        icao24, // Unique ICAO 24-bit address of the transponder in hex string representation.
        callsign, // Callsign of the vehicle (8 chars). Can be null if no callsign has been received.
        origin_country, // Country name inferred from the ICAO 24-bit address.
        time_position, // Unix timestamp (seconds) for the last position update. Can be null if no position report was received by OpenSky within the past 15s.
        last_contact, // Unix timestamp (seconds) for the last update in general. This field is updated for any new, valid message received from the transponder.
        longitude, // WGS-84 longitude in decimal degrees. Can be null.
        latitude, // WGS-84 latitude in decimal degrees. Can be null.
        baro_altitude, // Barometric altitude in meters. Can be null.
        on_ground,	// Boolean value which indicates if the position was retrieved from a surface position report.
        velocity,	// Velocity over ground in m/s. Can be null.
        true_track, // True track in decimal degrees clockwise from north (north=0°). Can be null.
        vertical_rate, // Vertical rate in m/s. A positive value indicates that the airplane is climbing, a negative value indicates that it descends. Can be null.
        sensors, // IDs of the receivers which contributed to this state vector. Is null if no filtering for sensor was used in the request.
        geo_altitude, // Geometric altitude in meters. Can be null.
        squawk, // The transponder code aka Squawk. Can be null.
        spi, // Whether flight status indicates special purpose indicator.
        position_source, // Origin of this state’s position: 0 = ADS-B, 1 = ASTERIX, 2 = MLAT
    }

    public record OpenSkyResult {
        public int time;
        public dynamic[] states;
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
            var planeResponse = await client.GetAsync("https://opensky-network.org/api/states/all");
            planeResponse.EnsureSuccessStatusCode();
            var planeData = await planeResponse.Content.ReadAsStringAsync();
            var options = new JsonSerializerOptions { IncludeFields = true, };
            return JsonSerializer.Deserialize<OpenSkyResult>(planeData, options);
        }

        private static IEnumerable<OpenSky> parseOpenSky(OpenSkyResult openSkyResult) {
            return openSkyResult.states.Where(state => 
                state[(int)OpenSkyFields.callsign].ValueKind == JsonValueKind.String &&
                state[(int)OpenSkyFields.latitude].ValueKind == JsonValueKind.Number &&
                state[(int)OpenSkyFields.longitude].ValueKind == JsonValueKind.Number &&
                state[(int)OpenSkyFields.baro_altitude].ValueKind == JsonValueKind.Number
            ).Select(state => new OpenSky {
                id = state[(int)OpenSkyFields.callsign].GetString(),
                latitude = state[(int)OpenSkyFields.latitude].GetDouble(),
                longitude = state[(int)OpenSkyFields.longitude].GetDouble(),
                altitude = state[(int)OpenSkyFields.baro_altitude].GetDouble(),
                true_track = state[(int)OpenSkyFields.true_track].GetDouble()
            });
        }

        [HttpGet]
        public IEnumerable<OpenSky> Get()
        {
            var data = Task.Run(async () => { return await GetDataFromOpenSkyAsync(); }).Result;
            return parseOpenSky(data);
        }
    }
}
