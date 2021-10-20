using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Mime;
using System.Threading.Tasks;
using CitizenDatabase.Models;
using CsvHelper;
using CsvHelper.Configuration;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CitizenDatabase.Controllers
{
    /// <summary>
    /// Citizen controller
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class CitizenController : ControllerBase
    {
        private readonly ILogger<CitizenController> _logger;
        private readonly Database.CitizenDatabase _db;

        public CitizenController(ILogger<CitizenController> logger, Database.CitizenDatabase db)
        {
            _logger = logger;
            _db = db;
        }

        /// <summary>
        /// Get citizen by ID
        /// </summary>
        /// <param name="id">ID of citizen</param>
        /// <returns>Citizen data</returns>
        [HttpGet("{id:int}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<Citizen>> Get(Int64 id)
        {
            try
            {
                var result = await _db.Citizens.FindAsync(id);
                return result == null ? NotFound() : Ok(result);
            }
            catch (Exception ex)
            {
                return DbError(ex);
            }
        }

        /// <summary>
        /// Add new citizen to database
        /// </summary>
        /// <param name="citizen">New citizen data</param>
        /// <returns>ID of created citizen</returns>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<int>> Add([Required] Citizen citizen)
        {
            citizen.Id = 0;
            try
            {
                await _db.Citizens.AddAsync(citizen);
                await _db.SaveChangesAsync();
                return citizen.Id == 0 ? BadRequest() : Ok(citizen.Id);
            }
            catch (Exception ex)
            {
                return DbError(ex);
            }
        }

        /// <summary>
        /// Update citizen data
        /// </summary>
        /// <param name="id">ID of citizen</param>
        /// <param name="citizen">Citizen data</param>
        [HttpPut("{id:int}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult> Update(Int64 id, [Required] Citizen citizen)
        {
            citizen.Id = id;
            try
            {
                _db.Citizens.Update(citizen);
                var rowCount = await _db.SaveChangesAsync();
                return rowCount > 0 ? Ok() : NotFound();
            }
            catch (Exception ex)
            {
                return DbError(ex);
            }
        }

        /// <summary>
        /// Remove citizen from database
        /// </summary>
        /// <param name="id">ID of citizen</param>
        [HttpDelete("{id:int}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult> Delete(Int64 id)
        {
            try
            {
                var result = await _db.Citizens.FindAsync(id);
                if (result == null)
                {
                    return NotFound();
                }

                _db.Citizens.Remove(result);
                var rowCount = await _db.SaveChangesAsync();
                return rowCount > 0 ? Ok() : NotFound();
            }
            catch (Exception ex)
            {
                return DbError(ex);
            }
        }

        /// <summary>
        /// Search citizens by pattern
        /// </summary>
        /// <param name="request">Search pattern</param>
        /// <returns>Found citizens</returns>
        [HttpGet]
        [Route("[action]")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<Citizen>>> Search([FromQuery] SearchRequest request)
        {
            try
            {
                var res = await FindByModel(request);
                return Ok(res);
            }
            catch (Exception ex)
            {
                return DbError(ex);
            }
        }

        /// <summary>
        /// Search citizens by pattern and export as CSV
        /// </summary>
        /// <param name="request">Search pattern</param>
        /// <returns>CSV result</returns>
        [HttpGet]
        [Route("[action]")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult> Export([FromQuery] SearchRequest request)
        {
            IEnumerable<Citizen> res;
            try
            {
                res = await FindByModel(request, false);
            }
            catch (Exception ex)
            {
                return DbError(ex);
            }

            await using var memoryStream = new MemoryStream();
            await using (var writer = new StreamWriter(memoryStream))
            await using (var csv = new CsvWriter(writer, new CsvConfiguration(CultureInfo.InvariantCulture) { HasHeaderRecord = true, Delimiter = ";" }))
            {
                await csv.WriteRecordsAsync(res);
            }

            return File(memoryStream.ToArray(), "text/csv");
        }

        /// <summary>
        /// Import new citizens from CSV
        /// </summary>
        /// <param name="scvFile">Csv file with citizens to add</param>
        [HttpPost]
        [Route("[action]")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult> Import(IFormFile scvFile)
        {
            using var reader = new StreamReader(scvFile.OpenReadStream());
            using var csv = new CsvReader(reader, new CsvConfiguration(CultureInfo.InvariantCulture) { HasHeaderRecord = true, Delimiter = ";" });

            var newCitizens = csv.GetRecords<Citizen>().ToArray();
            foreach (var newCitizen in newCitizens)
            {
                newCitizen.Id = 0;
            }

            try
            {
                await _db.AddRangeAsync(newCitizens);
                await _db.SaveChangesAsync();
                return Ok();
            }
            catch (Exception ex)
            {
                return DbError(ex);
            }
        }

        private async Task<IEnumerable<Citizen>> FindByModel(SearchRequest request, bool forcePaging = true)
        {
            var pageN = request.PageNumber ?? 0;
            var pageS = request.PageSize ?? 10;
            var query = _db.Citizens.Where(x =>
                    (request.FullName == null || x.FullName.StartsWith(request.FullName))
                    && (request.Inn == null || x.Inn.StartsWith(request.Inn))
                    && (request.Snils == null || x.Snils.StartsWith(request.Snils))
                    && (request.BirthDate == null || x.BirthDate == request.BirthDate)
                    && (request.DeathDate == null || x.DeathDate == request.DeathDate)
                );
            if (forcePaging || request.PageNumber.HasValue && request.PageSize.HasValue)
            {
                return await query.Skip(pageN * pageS).Take(pageS).ToArrayAsync();
            }
            else
            {
                return await query.ToArrayAsync();
            }
        }

        private ActionResult DbError(Exception ex)
        {
            _logger.LogError(ex, "Database error");
            return StatusCode(StatusCodes.Status500InternalServerError);
        }

    }
}