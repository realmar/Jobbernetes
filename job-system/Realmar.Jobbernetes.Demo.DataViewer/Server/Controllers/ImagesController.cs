using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using Realmar.Jobbernetes.Demo.Models;

namespace Realmar.Jobbernetes.Demo.DataViewer.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ImagesController : ControllerBase
    {
        private readonly IMongoCollection<ImageOutput> _collection;

        public ImagesController(IMongoCollection<ImageOutput> collection) => _collection = collection;

        [HttpGet]
        public IEnumerable<ImageOutput> Get() =>
            _collection.Find(FilterDefinition<ImageOutput>.Empty)
                       .Limit(100)
                       .Sort(new JsonSortDefinition<ImageOutput>("{'_id' : -1}"))
                       .ToEnumerable();
    }
}
